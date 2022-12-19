using System.Collections.Generic;
using UnityEngine;

public class PlayerUIUpgrades : MonoBehaviour {
    [SerializeField] Container<UpgradeCardData> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;

    readonly List<Container<UpgradeCardData>> _panels = new();

    static Player Owner => PlayerManager.LocalPlayer;

    void Awake() {
        for (var i = 0; i < Owner.Upgrades.Count; i++) {
            var upgrade = Owner.Upgrades[i];
            if (upgrade == null) continue;
            CreatePanel(upgrade, i);
        }
        Owner.OnUpgradeAdded += CreatePanel;
        Owner.OnUpgradeRemoved += RemovePanel;
    }

    void CreatePanel(UpgradeCardData data, int index) {
        var newPanel = Instantiate(_playerPanelPrefab, _playerPanelParent).SetContent(data);
        _panels.Insert(index, newPanel);
        newPanel.transform.SetSiblingIndex(index);
    }
    
    void RemovePanel(UpgradeCardData data, int index) {
        var panel = _panels[index];
        _panels.RemoveAt(index);
        Destroy(panel.gameObject);
    }
}