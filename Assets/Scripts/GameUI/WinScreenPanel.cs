using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenPanel : MonoBehaviour {
    [SerializeField] Image _icon;
    [SerializeField] Image _background;
    [Space]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Optional<TMP_Text> _robotText;
    [SerializeField] Optional<TMP_Text> _checkpointText;
    [Space]
    [SerializeField] Container<UpgradeCardData> _upgradePrefab;
    [SerializeField] Transform _upgradeParent;
    [SerializeField] bool _upgradesBeforeName;

    List<Transform> _objects;
    public IEnumerable<Transform> Objects => _objects;

    public WinScreenPanel SetContent(Player player) {
        _icon.sprite = player.RobotData.Icon;
        
        _nameText.text = player.ToString();
        if (_robotText.Enabled) {
            _robotText.Value.text = player.RobotData.Name;
        }
        _background.color = player.RobotData.Color;
        if (_checkpointText.Enabled) {
            _checkpointText.Value.text = $"#{player.CurrentCheckpoint.Value}";
        }

        _objects = new List<Transform> {
            _background.transform,
            _icon.transform
        };
        
        if (_upgradesBeforeName) CreateUpgrades();
        
        _objects.Add(_nameText.transform);
        if (_robotText.Value) {
            _objects.Add(_robotText.Value.transform);
        }
        if (_checkpointText.Value) {
            _objects.Add(_checkpointText.Value.transform);
        }
        
        if (!_upgradesBeforeName) CreateUpgrades();

        return this;
        
        void CreateUpgrades() {
            foreach (var upgrade in player.Upgrades) {
                if (upgrade == null) continue;
                _objects.Add(Instantiate(_upgradePrefab, _upgradeParent).SetContent(upgrade).transform);
            }   
        }
    }
}