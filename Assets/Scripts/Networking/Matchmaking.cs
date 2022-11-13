using System.Collections;
using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using ParrelSync;
#endif

public static class Matchmaking {
    public static UnityTransport Transport {
        get => _transport != null ? _transport : _transport = Object.FindObjectOfType<UnityTransport>();
        set => _transport = value;
    }

    static UnityTransport _transport;
    static Lobby _currentLobby;
    static string _localPlayerId;
    static Coroutine _heartbeatCoroutine;
    static Coroutine _refreshLobbyCoroutine;

    const string _relayJoinCodeKey = "j";
    const string _mapIDKey = "m";
    const int _heartbeatInterval = 15;
    const int _refreshLobbyInterval = 5;

    public static async Task InitializeAsync() {
        await InitializeUnityServicesAsync();
        await SignInAsync();
        Debug.Log("Matchmaking initialized");
    }

    static async Task InitializeUnityServicesAsync() {
        if (UnityServices.State == ServicesInitializationState.Uninitialized) {
            var options = new InitializationOptions();
#if UNITY_EDITOR
            if (ClonesManager.IsClone()) options.SetProfile(ClonesManager.GetArgument());
            else options.SetProfile("Primary");
#endif
            await UnityServices.InitializeAsync(options);
        }
    }

    public static async Task SignInAsync() {
        if (!AuthenticationService.Instance.IsSignedIn) {
            Debug.Log("Signing in...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn) {
                throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
            }
        }

        _localPlayerId = AuthenticationService.Instance.PlayerId;
        Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
    }

    public static async Task CreateLobbyAndAllocationAsync(LobbyData lobbyData) {
        if (_currentLobby != null) {
            Debug.LogError("Cannot join a lobby while already in a lobby");
            return;
        }

        var alloc = await RelayService.Instance.CreateAllocationAsync(lobbyData.MaxPlayers);
        var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

        var options = new CreateLobbyOptions {
            IsPrivate = lobbyData.IsPrivate,
            Data = new() {
                [_relayJoinCodeKey] = new(
                    visibility: DataObject.VisibilityOptions.Member,
                    index: DataObject.IndexOptions.S1,
                    value: relayJoinCode
                ),
                [_mapIDKey] = new(
                    visibility: DataObject.VisibilityOptions.Public,
                    index: DataObject.IndexOptions.N1,
                    value: lobbyData.MapID.ToString()
                )
            }
        };

        _currentLobby = await Lobbies.Instance.CreateLobbyAsync(
            $"Lobby {Random.Range(1000, 9999)}",
            lobbyData.MaxPlayers,
            options
        );

        Debug.Log($"Created lobby {_currentLobby.Id}, code: {_currentLobby.LobbyCode}");

        Transport.SetHostRelayData(
            alloc.RelayServer.IpV4,
            (ushort)alloc.RelayServer.Port,
            alloc.AllocationIdBytes,
            alloc.Key,
            alloc.ConnectionData
        );

        Transport.StartCoroutine(HeartbeatRoutine());
    }

    static IEnumerator HeartbeatRoutine() {
        while (_currentLobby != null) {
            Debug.Log($"Sending heartbeat to lobby {_currentLobby.Id}");
            Lobbies.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            yield return new WaitForSecondsRealtime(_heartbeatInterval);
        }
    }

    static IEnumerator RefreshLobbyRoutine() {
        while (_currentLobby != null) {
            Debug.Log($"Refreshing lobby {_currentLobby.Id}");
            var task = Lobbies.Instance.GetLobbyAsync(_currentLobby.Id);
            yield return new WaitUntil(() => task.IsCompleted);
            _currentLobby = task.Result;
            yield return new WaitForSecondsRealtime(_refreshLobbyInterval);
        }
    }

    public static async Task JoinLobbyWithCodeAsync(string lobbyCode) {
        if (_currentLobby != null) {
            throw new InvalidOperationException("Cannot join a lobby while already in a lobby");
        }

        _currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
        var relayJoinCode = _currentLobby.Data[_relayJoinCodeKey].Value;
        Debug.Log($"Joined lobby {_currentLobby.Id} using lobby code {_currentLobby.LobbyCode}");

        var alloc = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
        Transport.SetClientRelayData(
            alloc.RelayServer.IpV4,
            (ushort)alloc.RelayServer.Port,
            alloc.AllocationIdBytes,
            alloc.Key,
            alloc.ConnectionData,
            alloc.HostConnectionData
        );
    }

    public static async Task LockLobbyAsync() {
        try {
            await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
        } catch (Exception e) {
            Debug.Log($"Failed closing lobby: {e}");
        }
    }

    public static async Task UpdateLobbyAsync(UpdateLobbyDataOptions options) {
        try {
            var updateOpts = new UpdateLobbyOptions {
                IsPrivate = options.IsPrivate
            };

            updateOpts.Data[_mapIDKey] = new(
                visibility: DataObject.VisibilityOptions.Public,
                index: DataObject.IndexOptions.N1,
                value: options.MapID.ToString()
            );

            await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, updateOpts);
        } catch (Exception e) {
            Debug.Log($"Failed updating lobby: {e}");
        }
    }

    public static async Task LeaveLobbyAsync() {
        if (_currentLobby == null) {
            Debug.LogError("Cannot leave a lobby while not in a lobby");
            return;
        }

        if (_heartbeatCoroutine != null) {
            Transport.StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }

        try {
            if (_currentLobby.HostId == _localPlayerId) {
                await Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
            } else {
                await Lobbies.Instance.RemovePlayerAsync(_currentLobby.Id, _localPlayerId);
            }
            _currentLobby = null;
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to leave lobby {_currentLobby.Id}: {e.Message}");
        }
    }
}

public struct LobbyData {
    public byte MaxPlayers;
    public byte MapID;
    public bool IsPrivate;
}

public struct UpdateLobbyDataOptions {
    public byte? MapID;
    public bool? IsPrivate;
}