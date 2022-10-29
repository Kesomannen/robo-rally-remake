using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProgramCard : MonoBehaviour {
    [SerializeField] Image _artwork, _typeIcon;
    [SerializeField] Sprite _actionIcon, _damageIcon, _utilityIcon;
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _description;

    public ProgramCardData Data { get; private set; }

    public virtual void SetData(ProgramCardData data) {
        Data = data;
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