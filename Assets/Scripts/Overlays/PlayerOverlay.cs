using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOverlay : Overlay {
    [Header("References")]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _robotNameText;
    [SerializeField] Image _iconImage;
    [Space]
    [SerializeField] TMP_Text _energyText;
    [SerializeField] TMP_Text _checkpointText;
    [SerializeField] Transform _upgradeParent;

    [Header("Prefabs")]
    [SerializeField] Container<UpgradeCardData> _upgradeCardPrefab;

    [Header("Animation")]
    [SerializeField] GameObject[] _objects;
    [SerializeField] DynamicUITween _tween;

    Player _player;
    List<Container<UpgradeCardData>> _upgradeCards;

    public void Init(Player player) {
        _player = player;
        Refresh();
        StartCoroutine(TweenHelper.DoUITween(_tween, _objects.Concat(_upgradeCards.Select(c => c.gameObject))));
    }

    void Refresh() {
        _nameText.text = _player.ToString();
        _energyText.text = _player.Energy.ToString();
        _iconImage.sprite = _player.RobotData.Icon;
        _robotNameText.text = _player.RobotData.Name;
        _checkpointText.text = _player.CurrentCheckpoint.ToString();

        _upgradeCards = new List<Container<UpgradeCardData>>();
        foreach (var upgrade in _player.Upgrades) {
            if (upgrade == null) continue;
            _upgradeCards.Add(Instantiate(_upgradeCardPrefab, _upgradeParent).SetContent(upgrade));
        }
    }
}