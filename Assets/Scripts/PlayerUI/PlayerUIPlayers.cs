using UnityEngine;

public class PlayerUIPlayers : MonoBehaviour {
    [SerializeField] Container<Player> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] bool _showLocalPlayer = true;

    void Start(){
        foreach (var player in PlayerManager.Players){ 
            if (!_showLocalPlayer && PlayerManager.IsLocal(player)) continue;
            Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(player);
        }
    }
}