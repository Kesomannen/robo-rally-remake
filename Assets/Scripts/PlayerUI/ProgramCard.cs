using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ProgramCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("References")]
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
    [Space]
    [SerializeField] Sprite _standardSprite;
    [SerializeField] Sprite _highlightedSprite;

    public ProgramCardData Data { get; private set; }

    public virtual void OnPointerEnter(PointerEventData e)
    {
        _background.sprite = _highlightedSprite;
    }

    public virtual void OnPointerExit(PointerEventData e)
    {
        _background.sprite = _standardSprite;
    }

    public virtual void SetData(ProgramCardData data)
    {
        Data = data;
        _artwork.sprite = data.Artwork;
        _typeIcon.sprite = data.Type switch
        {
            ProgramCardData.CardType.Action => _actionIcon,
            ProgramCardData.CardType.Damage => _damageIcon,
            ProgramCardData.CardType.Utility => _utilityIcon,
            _ => throw new NotImplementedException()
        };
        _name.text = data.Name;
        _description.text = data.Description;
    }
}