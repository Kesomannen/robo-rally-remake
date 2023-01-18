using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class RoomMenu : Menu {
    [SerializeField] GameObject _startGameButton;
    [SerializeField] GameObject _readyButton;
    [SerializeField] LobbyPlayerPanel _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] TMP_Text _lobbyCodeText;

    public static bool LocalPlayerReady { get; private set; }
    
    readonly List<LobbyPlayerPanel> _playerPanels = new();
    
    public static event Action OnLocalPlayerReady; 

    public override void Show() {
        base.Show();

        _lobbyCodeText.text = LobbySystem.LobbyJoinCode;
        LocalPlayerReady = false;
        
        foreach (var lobbyPlayerPanel in _playerPanels) {
            Destroy(lobbyPlayerPanel.gameObject);
        }
        _playerPanels.Clear();
    
        _readyButton.gameObject.SetActive(true);
        _startGameButton.gameObject.SetActive(false);

        LobbySystem.OnPlayerUpdatedOrAdded += UpdatePanel;
        LobbySystem.OnPlayerRemoved += RemovePanel;

        // Do initial population of player panels
        foreach (var player in LobbySystem.PlayersInLobby) {
            UpdatePanel(player.Key, player.Value);
        }
    }

    public override async void Hide() {
        base.Hide();

        LobbySystem.OnPlayerUpdatedOrAdded -= UpdatePanel;
        LobbySystem.OnPlayerRemoved -= RemovePanel;
    
        await LobbySystem.Instance.LeaveLobby();
    }

    void UpdatePanel(ulong playerId, LobbyPlayerData playerData) {
        var playerPanel = _playerPanels.FirstOrDefault(x => x.PlayerId == playerId);
        if (playerPanel == null) {
            playerPanel = Instantiate(_playerPanelPrefab, _playerPanelParent);
            _playerPanels.Add(playerPanel);
        }
        playerPanel.SetData(playerId, playerData);

        _startGameButton.SetActive(
            NetworkManager.Singleton.IsHost &&
            LobbySystem.PlayersInLobby.Count >= GameSettings.Instance.MinPlayers &&
            LobbySystem.PlayersInLobby.All(p => p.Value.IsReady)
        );
    }

    void RemovePanel(ulong playerId) {
        var playerPanel = _playerPanels.FirstOrDefault(x => x.PlayerId == playerId);
        if (playerPanel == null) return;
        
        _playerPanels.Remove(playerPanel);
        Destroy(playerPanel.gameObject);
    }

    public void StartGame() {
        LobbySystem.Instance.StartGame();
    }

    public void Ready() {
        LobbySystem.Instance.UpdatePlayerData(ready: true);
        LocalPlayerReady = true;
        OnLocalPlayerReady?.Invoke();
        _readyButton.SetActive(false);
    }
    
    public void CopyInviteCode() {
        GUIUtility.systemCopyBuffer = LobbySystem.LobbyJoinCode;
    }
}