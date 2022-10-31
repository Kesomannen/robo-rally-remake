using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProgramCard : Container<ProgramCardData> {
    [SerializeField] Image _artwork;
    [SerializeField] Image _typeIcon;
    [SerializeField] Image _background;
    [Space]
    [SerializeField] Sprite _actionIcon;
    [SerializeField] Sprite _damageIcon;
    [SerializeField] Sprite _utilityIcon;
    [Space]
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _description;

    protected override void Serialize(ProgramCardData data) {
        _artwork.sprite = data.Artwork;
        _typeIcon.sprite = data.Type switch {
            ProgramCardData.CardType.Action => _actionIcon,
            ProgramCardData.CardType.Damage => _damageIcon,
            ProgramCardData.CardType.Utility => _utilityIcon,
            _ => throw new NotImplementedException()
        };
        _name.text = data.Name;
        _description.text = data.Description;
    }
}