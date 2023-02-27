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

    readonly List<LobbyPlayerPanel> _playerPanels = new();
    
    public static event Action OnLocalPlayerReady; 

    public override void Show() {
        base.Show();
        
        _lobbyCodeText.text = LobbySystem.LobbyJoinCode;

        foreach (var lobbyPlayerPanel in _playerPanels.Where(lobbyPlayerPanel => lobbyPlayerPanel != null)) {
            Destroy(lobbyPlayerPanel.gameObject);
        }
        _playerPanels.Clear();

        _readyButton.gameObject.SetActive(true);
        _startGameButton.gameObject.SetActive(false);

        LobbySystem.OnPlayerUpdatedOrAdded += UpdatePanel;
        LobbySystem.OnPlayerRemoved += RemovePanel;
    }

    public override async void Hide() {
        base.Hide();

        LobbySystem.OnPlayerUpdatedOrAdded -= UpdatePanel;
        LobbySystem.OnPlayerRemoved -= RemovePanel;
    
        using (new LoadScreen("Leaving Lobby...")) {
            await LobbySystem.Instance.LeaveLobby();
        }
    }

    void UpdatePanel(ulong playerId, LobbyPlayerData playerData) {
        var playerPanel = _playerPanels.FirstOrDefault(x => x.PlayerId == playerId);
        if (playerPanel == null) {
            playerPanel = Instantiate(_playerPanelPrefab, _playerPanelParent);
            _playerPanels.Add(playerPanel);
        }
        playerPanel.SetContent(playerId, playerData);

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

    public async void StartGame() {
        using (new LoadScreen("Staring Game...")) {
            await LobbySystem.Instance.StartGame();   
        }
    }

    public void Ready() {
        LobbySystem.Instance.UpdatePlayerData(ready: true);
        OnLocalPlayerReady?.Invoke();
        _readyButton.SetActive(false);
    }
    
    public void CopyInviteCode() {
        GUIUtility.systemCopyBuffer = LobbySystem.LobbyJoinCode;
    }
}