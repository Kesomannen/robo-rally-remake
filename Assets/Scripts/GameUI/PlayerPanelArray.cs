using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPanelArray : MonoBehaviour {
    [SerializeField] Container<Player> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] bool _showLocalPlayer = true;
    
    public List<Container<Player>> Panels { get; private set; }

    void OnEnable() {
        PlayerSystem.OnPlayerRemoved += OnPlayerRemoved;
    }
    
    void OnDisable() {
        PlayerSystem.OnPlayerRemoved -= OnPlayerRemoved;
    }
    
    void OnPlayerRemoved(Player player) {
        for (var i = 0; i < Panels.Count; i++) {
            if (Panels[i].Content != player) continue;
            Destroy(Panels[i].gameObject);
            Panels.RemoveAt(i);
            break;
        }
    }

    void Start() {
        Panels = new List<Container<Player>>(PlayerSystem.Players.Count);
        for (var i = 0; i < PlayerSystem.Players.Count; i++) {
            var player = PlayerSystem.Players[i];
            if (!_showLocalPlayer && PlayerSystem.IsLocal(player)) continue;
            Panels[i] = Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(player);
        }
    }
}