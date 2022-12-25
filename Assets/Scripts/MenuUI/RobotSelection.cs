using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RobotSelection : MonoBehaviour {
    [SerializeField] RobotPanel _robotPanelPrefab;
    [SerializeField] Transform _robotPanelParent;

    readonly Dictionary<RobotData, RobotPanel> _panels = new();
    Dictionary<ulong, RobotData> _playerRobots;

    void Start() {
        _playerRobots = LobbySystem.PlayersInLobby.ToDictionary(
            k => k.Key,
            k => RobotData.GetById(k.Value.RobotId)
        );

        foreach (var robotData in RobotData.GetAll()) {
            var newPanel = Instantiate(_robotPanelPrefab, _robotPanelParent);
            newPanel.SetContent(robotData);
            
            RefreshPanel(newPanel);
            _panels.Add(robotData, newPanel);
        }
    }

    void OnEnable() {
        _playerRobots = new Dictionary<ulong, RobotData>();
        
        LobbySystem.OnPlayerUpdatedOrAdded += OnPlayerUpdatedOrAdded;
        LobbySystem.OnPlayerRemoved += OnPlayerRemoved;
    }
    
    void OnDisable() {
        LobbySystem.OnPlayerUpdatedOrAdded -= OnPlayerUpdatedOrAdded;
        LobbySystem.OnPlayerRemoved -= OnPlayerRemoved;
    }
    
    void OnPlayerRemoved(ulong id) {
        var robotData = _playerRobots[id];
        _playerRobots.Remove(id);
        
        RefreshPanel(robotData);
    }

    void OnPlayerUpdatedOrAdded(ulong id, LobbyPlayerData data) {
        var robotData = RobotData.GetById(data.RobotId);
        _playerRobots[id] = robotData;

        foreach (var (_, panel) in _panels) {
            RefreshPanel(panel);
        }
    }

    void RefreshPanel(RobotData panelContent) => RefreshPanel(_panels[panelContent]);

    void RefreshPanel(RobotPanel panel) {
        var localId = NetworkManager.Singleton.LocalClientId;
        panel.Interactable = !LobbySystem.PlayersInLobby[localId].IsReady 
                             && !_playerRobots.Any(k => k.Key != localId 
                                                        && k.Value == panel.Content);
    }
}