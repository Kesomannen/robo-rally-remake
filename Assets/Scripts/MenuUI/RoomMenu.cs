using System;
using System.Collections;
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
    [SerializeField] SettingsPanelProperty _startingEnergyProperty, _cardsPerTurnProperty, _stressTimeProperty, _upgradeSlotsProperty, _beginnerGameProperty, _advancedGameProperty, _gameSpeedProperty;

    readonly List<LobbyPlayerPanel> _playerPanels = new();
    
    public static event Action LocalPlayerReady;

    public override void Show() {
        base.Show();
        
        _lobbyCodeText.text = LobbySystem.LobbyJoinCode;

        foreach (var lobbyPlayerPanel in _playerPanels.Where(lobbyPlayerPanel => lobbyPlayerPanel != null)) {
            Destroy(lobbyPlayerPanel.gameObject);
        }
        _playerPanels.Clear();
        
        foreach (var (id, data) in LobbySystem.PlayersInLobby) {
            UpdatePanel(id, data);
        }
        
        var settings = LobbySystem.GameSettings;
        _startingEnergyProperty.GameProperty = settings.StartingEnergy;
        _cardsPerTurnProperty.GameProperty = settings.CardsPerTurn;
        _stressTimeProperty.GameProperty = settings.StressTime;
        _upgradeSlotsProperty.GameProperty = settings.UpgradeSlots;
        _beginnerGameProperty.GameProperty = settings.BeginnerGame;
        _advancedGameProperty.GameProperty = settings.AdvancedGame;
        _gameSpeedProperty.GameProperty = settings.GameSpeed;

        _readyButton.gameObject.SetActive(true);
        _startGameButton.gameObject.SetActive(false);
    }

    void OnDisable() {
        LobbySystem.PlayerUpdatedOrAdded -= UpdatePanel;
        LobbySystem.PlayerRemoved -= RemovePanel;
    }

    void OnEnable() {
        LobbySystem.PlayerUpdatedOrAdded += UpdatePanel;
        LobbySystem.PlayerRemoved += RemovePanel;
    }

    public override async void Hide() {
        base.Hide();

        using (new LoadingScreen("Leaving Lobby...")) {
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
            LobbySystem.PlayersInLobby.Count >= LobbySystem.MinPlayers &&
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
        StartCoroutine(StartGameCoroutine());
        
        IEnumerator StartGameCoroutine() {
            using (new LoadingScreen("Starting Game...")) {
                yield return LobbySystem.Instance.StartGame();
            }
        }
    }

    public void Ready() {
        LobbySystem.Instance.UpdatePlayer(ready: true);
        LocalPlayerReady?.Invoke();
        _readyButton.SetActive(false);
    }
    
    public void CopyInviteCode() {
        GUIUtility.systemCopyBuffer = LobbySystem.LobbyJoinCode;
    }
}