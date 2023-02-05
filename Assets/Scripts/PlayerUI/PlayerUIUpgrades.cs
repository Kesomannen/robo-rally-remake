using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerUIUpgrades : MonoBehaviour {
    [FormerlySerializedAs("_playerPanelPrefab")] 
    [SerializeField] Container<UpgradeCardData> _upgradePanelPrefab;
    
    [FormerlySerializedAs("_playerPanelParent")] 
    [SerializeField] Transform _upgradePanelParent;

    [FormerlySerializedAs("_playerPanelSize")] 
    [SerializeField] Vector2 _upgradePanelSize;
    
    [SerializeField] Transform _highlightParent;

    readonly Dictionary<Container<UpgradeCardData>, int> _cards = new();

    static Player Owner => PlayerSystem.LocalPlayer;

    void Awake() {
        for (var i = 0; i < Owner.Upgrades.Count; i++) {
            var upgrade = Owner.Upgrades[i];
            if (upgrade == null) continue;
            CreatePanel(upgrade, i);
        }
        Owner.OnUpgradeAdded += CreatePanel;
        Owner.OnUpgradeRemoved += RemovePanel;
    }

    void OnDestroy() {
        Owner.OnUpgradeAdded -= CreatePanel;
        Owner.OnUpgradeRemoved -= RemovePanel;
    }

    void CreatePanel(UpgradeCardData data, int index) {
        var newPanel = Instantiate(_upgradePanelPrefab, _upgradePanelParent);
        newPanel.SetContent(data);
        newPanel.GetComponent<HandUpgradeCard>().HighlightParent = _highlightParent;
        _cards.Add(newPanel, index);
        
        newPanel.transform.SetSiblingIndex(index);
        UpdatePositions();
    }
    
    void RemovePanel(UpgradeCardData data, int index) {
        var panel = _cards.FirstOrDefault(c => c.Value == index).Key;
        Destroy(panel.gameObject);
        UpdatePositions();
    }
    
    void UpdatePositions() {
        var rows = Mathf.CeilToInt(_cards.Count / 3f);
        var columns = Mathf.CeilToInt(_cards.Count / (float)rows);
        
        var i = 0;
        for (var row = 0; row < rows; row++) {
            for (var column = 0; column < columns; column++) {
                if (i >= _cards.Count) return;
                
                var panel = _cards.ElementAt(i).Key;
                var position = new Vector2(column * _upgradePanelSize.x, -row * _upgradePanelSize.y);
                panel.transform.localPosition = position;
                panel.transform.SetSiblingIndex(i);
                i++;
            }
        }
    }
}