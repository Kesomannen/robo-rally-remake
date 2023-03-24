using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] UpgradeCardData[] _upgradesToLoad;
    [SerializeField] Optional<MapData> _mapToLoad;

    void Awake() {
        if (NetworkSystem.LoadContext != NetworkSystem.Context.Singleplayer) {
            Destroy(this);
            return;
        }
        
        if (_mapToLoad.Enabled) {
            LobbySystem.LobbyMap.Value = _mapToLoad.Value.GetLookupId();
        }
    }

    void Start() {
        foreach (var upgrade in _upgradesToLoad) {
            PlayerSystem.Players[0].ReplaceUpgradeAt(upgrade, 0);
        }
    }
}