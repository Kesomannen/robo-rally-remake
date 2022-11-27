using System.Collections;
using System;
using System.Collections.Generic;
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

    public static Lobby CurrentLobby { get; private set; }
    
    static UnityTransport _transport;
    static string _localPlayerId;
    static Coroutine _heartbeatCoroutine;
    static Coroutine _refreshLobbyCoroutine;

    const string RelayJoinCodeKey = "j";
    const string MapIDKey = "m";
    const int HeartbeatInterval = 15;
    const int RefreshLobbyInterval = 5;

    public static int GetMapID() {
        return CurrentLobby == null ? 0 : int.Parse(CurrentLobby.Data[MapIDKey].Value);
    }
    
    public static async Task InitializeAsync() {
        await InitializeUnityServicesAsync();
        await SignInAsync();
        Debug.Log("Matchmaking initialized");
    }

    static async Task InitializeUnityServicesAsync() {
        if (UnityServices.State == ServicesInitializationState.Uninitialized) {
            var options = new InitializationOptions();
#if UNITY_EDITOR
            options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
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
        if (CurrentLobby != null) {
            Debug.LogError("Cannot join a lobby while already in a lobby");
            return;
        }

        var alloc = await RelayService.Instance.CreateAllocationAsync(lobbyData.MaxPlayers);
        var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

        var options = new CreateLobbyOptions {
            IsPrivate = lobbyData.IsPrivate,
            Data = new Dictionary<string, DataObject> {
                [RelayJoinCodeKey] = new(
                    visibility: DataObject.VisibilityOptions.Member,
                    index: DataObject.IndexOptions.S1,
                    value: relayJoinCode
                ),
                [MapIDKey] = new(
                    visibility: DataObject.VisibilityOptions.Public,
                    index: DataObject.IndexOptions.N1,
                    value: lobbyData.MapID.ToString()
                )
            }
        };

        CurrentLobby = await Lobbies.Instance.CreateLobbyAsync(
            $"Lobby {Random.Range(1000, 9999)}",
            lobbyData.MaxPlayers,
            options
        );

        Debug.Log($"Created lobby {CurrentLobby.Id}, code: {CurrentLobby.LobbyCode}");

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
        while (CurrentLobby != null) {
            Debug.Log($"Sending heartbeat to lobby {CurrentLobby.Id}");
            Lobbies.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            yield return new WaitForSecondsRealtime(HeartbeatInterval);
        }
    }

    static IEnumerator RefreshLobbyRoutine() {
        while (CurrentLobby != null) {
            Debug.Log($"Refreshing lobby {CurrentLobby.Id}");
            var task = Lobbies.Instance.GetLobbyAsync(CurrentLobby.Id);
            yield return new WaitUntil(() => task.IsCompleted);
            CurrentLobby = task.Result;
            yield return new WaitForSecondsRealtime(RefreshLobbyInterval);
        }
    }

    public static async Task JoinLobbyWithCodeAsync(string lobbyCode) {
        if (CurrentLobby != null) {
            throw new InvalidOperationException("Cannot join a lobby while already in a lobby");
        }

        CurrentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
        var relayJoinCode = CurrentLobby.Data[RelayJoinCodeKey].Value;
        Debug.Log($"Joined lobby {CurrentLobby.Id} using lobby code {CurrentLobby.LobbyCode}");

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
            await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
        } catch (Exception e) {
            Debug.Log($"Failed closing lobby: {e}");
        }
    }

    public static async Task UpdateLobbyAsync(UpdateLobbyDataOptions options) {
        try {
            var updateOpts = new UpdateLobbyOptions {
                IsPrivate = options.IsPrivate
            };

            updateOpts.Data[MapIDKey] = new DataObject(
                visibility: DataObject.VisibilityOptions.Public,
                index: DataObject.IndexOptions.N1,
                value: options.MapID.ToString()
            );

            await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOpts);
        } catch (Exception e) {
            Debug.Log($"Failed updating lobby: {e}");
        }
    }

    public static async Task LeaveLobbyAsync() {
        if (CurrentLobby == null) {
            Debug.LogError("Cannot leave a lobby while not in a lobby");
            return;
        }

        if (_heartbeatCoroutine != null) {
            Transport.StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }

        try {
            if (CurrentLobby.HostId == _localPlayerId) {
                await Lobbies.Instance.DeleteLobbyAsync(CurrentLobby.Id);
            } else {
                await Lobbies.Instance.RemovePlayerAsync(CurrentLobby.Id, _localPlayerId);
            }
            CurrentLobby = null;
        } catch (LobbyServiceException e) {
            Debug.LogError($"Failed to leave lobby {CurrentLobby.Id}: {e.Message}");
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