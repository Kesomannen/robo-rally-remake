using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Container<>))]
public class ChoiceItem<T> : MonoBehaviour, IPointerClickHandler {
    [Header("References")]
    [SerializeField] Selectable _selectable;
    [SerializeField] GameObject _unavailableOverlay;
    [SerializeField] Container<T> _container;

    public Container<T> Container => _container;

    public static Action<T> OnCardSelected;

    bool _isAvailable;

    public void OnPointerClick(PointerEventData e) {
        if (!_isAvailable) return;
        OnCardSelected?.Invoke(Container.Content);
    }

    public void SetAvailable(bool available) {
        if (_isAvailable == available) return;
        _isAvailable = available;

        _selectable.interactable = available;
        _unavailableOverlay.SetActive(!available);
    }
}