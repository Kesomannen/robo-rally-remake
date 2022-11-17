using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProgramCard : Container<ProgramCardData> {
    [SerializeField] Image _artwork;
    [SerializeField] Image _typeIcon;
    [Space]
    [SerializeField] Sprite _actionIcon;
    [SerializeField] Sprite _damageIcon;
    [SerializeField] Sprite _utilityIcon;
    [Space]
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _description;

    protected override void Serialize(ProgramCardData player) {
        _artwork.sprite = player.Artwork;
        _typeIcon.sprite = player.Type switch {
            ProgramCardData.CardType.Action => _actionIcon,
            ProgramCardData.CardType.Damage => _damageIcon,
            ProgramCardData.CardType.Utility => _utilityIcon,
            _ => throw new NotImplementedException()
        };
        _name.text = player.Name;
        _description.text = player.Description;
    }
}