using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProgramCardChoiceItem : ProgramCard, IPointerClickHandler {
    [SerializeField] Selectable _selectable;
    [SerializeField] GameObject _unavailableOverlay;

    public static Action<ProgramCardData> OnCardSelected;

    bool _isAvailable;

    public void OnPointerClick(PointerEventData e) {
        if (!_isAvailable) return;
        OnCardSelected?.Invoke(Data);
    }

    public void SetAvailable(bool available) {
        if (_isAvailable == available) return;
        _isAvailable = available;

        _selectable.interactable = available;
        _unavailableOverlay.SetActive(!available);
    }
}