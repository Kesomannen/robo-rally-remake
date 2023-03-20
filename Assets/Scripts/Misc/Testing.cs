using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] UpgradeCardData[] _upgradesToLoad;
    
    void Start() {
        if (NetworkSystem.LoadContext != NetworkSystem.Context.Singleplayer) {
            Destroy(this);
            return;
        }

        foreach (var upgrade in _upgradesToLoad) {
            PlayerSystem.Players[0].ReplaceUpgradeAt(upgrade, 0);
        }
    }
}