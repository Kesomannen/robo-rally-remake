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

#if UNITY_EDITOR
using ParrelSync;
#endif

public static class Matchmaking {
    public static Player LocalPlayer { get; private set; }

    public static UnityTransport Transport {
        get => _transport != null ? _transport : _transport = Object.FindObjectOfType<UnityTransport>();
        set => _transport = value;
    }

    static UnityTransport _transport;
    static Lobby _currentLobby;
    static Coroutine _heartbeatCoroutine;
    static Coroutine _refreshLobbyCoroutine;

    const string _relayJoinCodeKey = "j";
    const string _mapIDKey = "m";
    const int _heartbeatInterval = 15;
    const int _refreshLobbyInterval = 5;

    public static async Task InitializeAsync() {
        await InitializeUnityServicesAsync();
        LocalPlayer = await SignInAndGetPlayerAsync();
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

    public static async Task<Player> SignInAndGetPlayerAsync() {
        if (!AuthenticationService.Instance.IsSignedIn) {
            Debug.Log("Signing in...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn) {
                throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
            }
        }

        Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");

        return new Player(
            id: AuthenticationService.Instance.PlayerId,
            connectionInfo: null,
            data: new()
        );
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
            lobbyData.Name,
            lobbyData.MaxPlayers,
            options
        );

        Debug.Log($"Created lobby {_currentLobby.Id} with relay code: {relayJoinCode}");

        Transport.SetHostRelayData(
            alloc.RelayServer.IpV4,
            (ushort) alloc.RelayServer.Port,
            alloc.AllocationIdBytes,
            alloc.Key,
            alloc.ConnectionData
        );

        Transport.StartCoroutine(HeartbeatCoroutine());
    }

    static IEnumerator HeartbeatCoroutine() {
        while (_currentLobby != null) {
            Lobbies.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            yield return new WaitForSecondsRealtime(_heartbeatInterval);
        }
    }

    static IEnumerator RefreshLobbyCoroutine() {
        while (_currentLobby != null) {
            var task = Lobbies.Instance.GetLobbyAsync(_currentLobby.Id);
            yield return new WaitUntil(() => task.IsCompleted);
            _currentLobby = task.Result;
            yield return new WaitForSecondsRealtime(_refreshLobbyInterval);
        }
    }

    public static async Task<bool> JoinLobbyWithCodeAsync(string lobbyCode) {
        if (_currentLobby != null) {
            Debug.LogError("Cannot join a lobby while already in a lobby");
            return false;
        }

        try {
            _currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            var relayJoinCode = _currentLobby.Data[_relayJoinCodeKey].Value;
            Debug.Log($"Joined lobby {_currentLobby.Id} using lobby code {_currentLobby.LobbyCode}");

            var alloc = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            Transport.SetClientRelayData (
                alloc.RelayServer.IpV4,
                (ushort) alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
            );

            return true;
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to join lobby with code {lobbyCode}: {e.Message}");
            return false;
        }
    }

    public static async Task LockLobbyAsync() {
        try {
            await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
        }
        catch (Exception e) {
            Debug.Log($"Failed closing lobby: {e}");
        }
    }

    public static async Task UpdateLobbyAsync(UpdateLobbyDataOptions options) {
        try {
            var updateOpts = new UpdateLobbyOptions();
            if (options.MaxPlayers != null) updateOpts.MaxPlayers = options.MaxPlayers;
            if (options.IsPrivate != null) updateOpts.IsPrivate = options.IsPrivate;
            if (options.MapID != null) updateOpts.Data[_mapIDKey] = new(
                visibility: DataObject.VisibilityOptions.Public,
                index: DataObject.IndexOptions.N1,
                value: options.MapID.ToString()
            );

            await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, updateOpts);
        }
        catch (Exception e) {
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
            if (_currentLobby.HostId == LocalPlayer.Id) {
                await Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
            } else {
                await Lobbies.Instance.RemovePlayerAsync(_currentLobby.Id, LocalPlayer.Id);
            } 
            _currentLobby = null;
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to leave lobby {_currentLobby.Id}: {e.Message}");
        }
    }
}

public struct LobbyData {
    public int MaxPlayers;
    public int MapID;
    public bool IsPrivate;
    public string Name;
}

public struct UpdateLobbyDataOptions {
    public int? MaxPlayers;
    public int? MapID;
    public bool? IsPrivate;
}