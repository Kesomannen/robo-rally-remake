using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoomMenu : Menu {
    [SerializeField] GameObject _startGameButton;
    [SerializeField] GameObject _readyButton;
    [SerializeField] LobbyPlayerPanel _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;

    readonly List<LobbyPlayerPanel> _playerPanels = new();

    protected override MenuState PreviousMenuState => MenuState.JoinGame;

    public override void Show() {
        base.Show();

        foreach (var lobbyPlayerPanel in _playerPanels) {
            Destroy(lobbyPlayerPanel.gameObject);
        }
        _playerPanels.Clear();
    
        _readyButton.gameObject.SetActive(true);
        _startGameButton.gameObject.SetActive(false);

        LobbySystem.OnPlayerUpdatedOrAdded += UpdatePanel;
        LobbySystem.OnPlayerRemoved += RemovePanel;

        // Do initial population of player panels
        foreach (var player in LobbySystem.Instance.PlayersInLobby) {
            UpdatePanel(player.Key, player.Value);
        }
    }

    void UpdatePanel(ulong playerId, PlayerData playerData) {
        var playerPanel = _playerPanels.FirstOrDefault(x => x.PlayerId == playerId);
        if (playerPanel == null) {
            playerPanel = Instantiate(_playerPanelPrefab, _playerPanelParent);
            _playerPanels.Add(playerPanel);
        }
        playerPanel.SetData(playerId, playerData);

        _startGameButton.SetActive(
            NetworkManager.Singleton.IsHost &&
            LobbySystem.Instance.PlayersInLobby.All(p => p.Value.IsReady)
        );
    }

    void RemovePanel(ulong playerId) {
        var playerPanel = _playerPanels.FirstOrDefault(x => x.PlayerId == playerId);
        if (playerPanel != null) {
            _playerPanels.Remove(playerPanel);
            Destroy(playerPanel.gameObject);
        }
    }

    public async override void Hide() {
        LobbySystem.OnPlayerUpdatedOrAdded -= UpdatePanel;
        LobbySystem.OnPlayerRemoved -= RemovePanel;
    
        await LobbySystem.Instance.LeaveLobby();
        base.Hide();
    }

    public void StartGame() {
        LobbySystem.Instance.StartGame();
    }

    public void Ready() {
        LobbySystem.Instance.UpdatePlayerData(ready: true);
        _readyButton.SetActive(false);
    }
}