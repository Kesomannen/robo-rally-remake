using System.Collections.Generic;
using UnityEngine;

public class PlayerUIUpgrades : MonoBehaviour {
    [SerializeField] Container<UpgradeCardData> _playerPanelPrefab;
    [SerializeField] Transform _playerPanelParent;
    [SerializeField] Transform _highlightParent;
    [SerializeField] Vector2 _playerPanelSize;

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
        var newPanel = Instantiate(_playerPanelPrefab, _playerPanelParent);
        newPanel.SetContent(data);
        newPanel.GetComponent<PlayerUpgradeCard>().HighlightParent = _highlightParent;
        _panels.Insert(index, newPanel);
        
        newPanel.transform.SetSiblingIndex(index);
        UpdatePositions();
    }
    
    void RemovePanel(UpgradeCardData data, int index) {
        var panel = _panels[index];
        _panels.RemoveAt(index);
        Destroy(panel.gameObject);
        UpdatePositions();
    }
    
    void UpdatePositions() {
        var rows = Mathf.CeilToInt(_panels.Count / 3f);
        var columns = Mathf.CeilToInt(_panels.Count / (float)rows);
        
        var i = 0;
        for (var row = 0; row < rows; row++) {
            for (var column = 0; column < columns; column++) {
                if (i >= _panels.Count) return;
                
                var panel = _panels[i];
                var position = new Vector2(column * _playerPanelSize.x, -row * _playerPanelSize.y);
                panel.transform.localPosition = position;
                panel.transform.SetSiblingIndex(i);
                i++;
            }
        }
    }
}