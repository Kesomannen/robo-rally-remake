using UnityEngine;

public class PlayerPanelArray : MonoBehaviour {
    [SerializeField] Container<Player> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] bool _showLocalPlayer = true;

    void Start() {
        foreach (var player in PlayerSystem.Players) { 
            if (!_showLocalPlayer && PlayerSystem.IsLocal(player)) continue;
            Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(player);
        }
    }
}