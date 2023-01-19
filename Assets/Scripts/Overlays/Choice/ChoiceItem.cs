using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Container<>))]
public class ChoiceItem<T> : MonoBehaviour, IPointerClickHandler {
    [Header("References")]
    [SerializeField] Selectable _selectable;
    [SerializeField] GameObject _unavailableOverlay;

    public Container<T> Container { get; private set; }

    public static Action<T> OnCardSelected;

    bool _isAvailable;

    void Awake() {
        Container = GetComponent<Container<T>>();
    }

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