using System;
using UnityEngine;

public class PlayerUIPlayers : MonoBehaviour {
    [SerializeField] Container<Player> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;

    void Start(){
        foreach (var player in PlayerManager.Players){
            Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(player);
        }
    }
}