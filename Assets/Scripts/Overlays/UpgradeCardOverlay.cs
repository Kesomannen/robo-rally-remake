using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeCardOverlay : Overlay {
    [Header("References")]
    [FormerlySerializedAs("_container")]
    [SerializeField] Container<UpgradeCardData> _upgradeContainer;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Transform _tooltipParent;
    
    [Header("Prefabs")]
    [SerializeField] UpgradeTooltip _textPrefab;
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] UpgradeCard _upgradeCardPrefab;

    [Header("Animation")]
    [SerializeField] DynamicUITween _onEnableTween;

    public void Init(UpgradeCardData upgrade) {
        var objects = new List<GameObject>(2 + upgrade.Tooltips.Count()) {
            _upgradeContainer.gameObject,
            _descriptionText.gameObject,
        };

        _upgradeContainer.SetContent(upgrade);
        _descriptionText.text = upgrade.Description;
        objects.AddRange(upgrade.Tooltips.Select(CreateTooltip));

        StartCoroutine(TweenHelper.DoUITween(_onEnableTween, objects));
        
        GameObject CreateTooltip(UpgradeTooltipData data) {
            switch (data.Type){
                case UpgradeTooltipType.Text:
                    var text = Instantiate(_textPrefab, _tooltipParent);
                    text.SetContent(data.Header, data.Description);
                    return text.gameObject;
                
                case UpgradeTooltipType.Mechanic:
                    var (header, description) = UpgradeTooltipData.GetMechanic(data.Mechanic);
                    var mechanic = Instantiate(_textPrefab, _tooltipParent);
                    mechanic.SetContent(header, description);
                    return mechanic.gameObject;
                
                case UpgradeTooltipType.ProgramCard:
                    var programCard = Instantiate(_programCardPrefab, _tooltipParent);
                    programCard.SetContent(data.ProgramCard);
                    return programCard.gameObject;
                
                case UpgradeTooltipType.UpgradeCard:
                    var upgradeCard = Instantiate(_upgradeCardPrefab, _tooltipParent);
                    upgradeCard.SetContent(data.UpgradeCard);
                    return upgradeCard.gameObject;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}