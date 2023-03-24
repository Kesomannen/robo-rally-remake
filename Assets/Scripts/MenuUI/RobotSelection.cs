using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RobotSelection : MonoBehaviour {
    [SerializeField] RobotPanel _robotPanelPrefab;
    [SerializeField] Transform _robotPanelParent;
    [SerializeField] RobotData[] _availableRobots;

    readonly List<RobotPanel> _panels = new();
    Dictionary<ulong, RobotData> _playerRobots;

    void Awake() {
        foreach (var robotData in _availableRobots) {
            var newPanel = Instantiate(_robotPanelPrefab, _robotPanelParent);
            newPanel.SetContent(robotData);
            _panels.Add(newPanel);
        }
    }
    
    void OnEnable() {
        LobbySystem.PlayerUpdatedOrAdded += OnPlayerUpdatedOrAdded;
        LobbySystem.PlayerRemoved += OnPlayerRemoved;
        RoomMenu.LocalPlayerReady += OnLocalPlayerReady;
        
        LeanTween.delayedCall(0.5f, () => {
            _playerRobots = LobbySystem.PlayersInLobby.ToDictionary(pair => pair.Key, pair => RobotData.GetById(pair.Value.RobotId));
            UpdatePanels();
        });
    }

    void OnDisable() {
        LobbySystem.PlayerUpdatedOrAdded -= OnPlayerUpdatedOrAdded;
        LobbySystem.PlayerRemoved -= OnPlayerRemoved;
        RoomMenu.LocalPlayerReady -= OnLocalPlayerReady;
    }
    
    void OnLocalPlayerReady() {
        var localRobot = _playerRobots[NetworkManager.Singleton.LocalClientId];
        foreach (var panel in _panels) {
            panel.SetState(panel.Content == localRobot ? RobotPanel.State.Selected : RobotPanel.State.Unavailable);
        }
    }

    void OnPlayerRemoved(ulong id) {
        _playerRobots.Remove(id);
        UpdatePanels();
    }
    
    void OnPlayerUpdatedOrAdded(ulong id, LobbyPlayerData data) {
        _playerRobots[id] = RobotData.GetById(data.RobotId);
        UpdatePanels();
    }

    void UpdatePanel(RobotPanel panel) {
        var localId = NetworkManager.Singleton.LocalClientId;

        if (_playerRobots[localId] == panel.Content) {
            panel.SetState(RobotPanel.State.Selected);
        } else if (_playerRobots.Any(pair => pair.Value == panel.Content) 
                   || LobbySystem.PlayersInLobby[localId].IsReady) {
            panel.SetState(RobotPanel.State.Unavailable);
        } else {
            panel.SetState(RobotPanel.State.Available);
        }
    }
    
    void UpdatePanels() {
        foreach (var panel in _panels) {
            UpdatePanel(panel);
        }
    }
}