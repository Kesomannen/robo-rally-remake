using UnityEngine;

public class PlayerPanelArray : MonoBehaviour {
    [SerializeField] Container<Player> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] bool _showLocalPlayer = true;
    
    public Container<Player>[] Panels { get; private set; }

    void Start() {
        Panels = new Container<Player>[PlayerSystem.Players.Count];
        for (var i = 0; i < PlayerSystem.Players.Count; i++) {
            var player = PlayerSystem.Players[i];
            if (!_showLocalPlayer && PlayerSystem.IsLocal(player)) continue;
            Panels[i] = Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(player);
        }
    }
}