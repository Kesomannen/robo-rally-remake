using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RobotSelection : MonoBehaviour {
    [SerializeField] RobotPanel _robotPanelPrefab;
    [SerializeField] Transform _robotPanelParent;

    readonly List<RobotPanel> _panels = new();
    readonly Dictionary<ulong, RobotData> _playerRobots = new();

    void Awake() {
        foreach (var robotData in RobotData.GetAll()) {
            var newPanel = Instantiate(_robotPanelPrefab, _robotPanelParent);
            newPanel.SetContent(robotData);
            _panels.Add(newPanel);
        }
    }
    
    void OnEnable() {
        _playerRobots.Clear();

        LobbySystem.OnPlayerUpdatedOrAdded += OnPlayerUpdatedOrAdded;
        LobbySystem.OnPlayerRemoved += OnPlayerRemoved;
        RoomMenu.OnLocalPlayerReady += OnLocalPlayerReady;
        
        _panels.ForEach(UpdatePanel);
    }

    void OnDisable() {
        LobbySystem.OnPlayerUpdatedOrAdded -= OnPlayerUpdatedOrAdded;
        LobbySystem.OnPlayerRemoved -= OnPlayerRemoved;
        RoomMenu.OnLocalPlayerReady -= OnLocalPlayerReady;
    }
    
    void OnLocalPlayerReady() {
        _panels.ForEach(UpdatePanel);
    }

    void OnPlayerRemoved(ulong id) {
        UpdatePanel(_playerRobots[id]);
        _playerRobots.Remove(id);
    }
    
    void OnPlayerUpdatedOrAdded(ulong id, LobbyPlayerData data) {
        var isUpdate = _playerRobots.TryGetValue(id, out var prev);

        _playerRobots[id] = RobotData.GetById(data.RobotId);
        if (isUpdate) UpdatePanel(prev);
        UpdatePanel(_playerRobots[id]);
    }

    void UpdatePanel(RobotData robotData) => UpdatePanel(_panels.First(p => p.Content == robotData));

    void UpdatePanel(RobotPanel panel) {
        var localId = NetworkManager.Singleton.LocalClientId;

        if (_playerRobots[localId] == panel.Content) {
            panel.SetState(RobotPanel.State.Selected);
        } else if (_playerRobots.Any(player => player.Value == panel.Content) 
                   || LobbySystem.PlayersInLobby[localId].IsReady) {
            panel.SetState(RobotPanel.State.Unavailable);
        } else {
            panel.SetState(RobotPanel.State.Available);
        }
    }
}