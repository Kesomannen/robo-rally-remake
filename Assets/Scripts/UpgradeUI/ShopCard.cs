using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopCard : UpgradeCard, IPointerClickHandler, IPointerEnterHandler {
    [SerializeField] GameObject _unavailableOverlay;
    [SerializeField] float _buyTweenDuration = 0.5f;
    [SerializeField] LeanTweenType _buyTweenType;
    [SerializeField] LayoutElement _layoutElement;

    public bool Available {
        get {
            var localPlayer = PlayerSystem.LocalPlayer;

            return gameObject.activeInHierarchy
                   && ShopPhase.CurrentPlayer == localPlayer
                   && Content.Cost <= localPlayer.Energy.Value
                   && !localPlayer.Upgrades.Contains(Content);
        }
    }

    public event Action<ShopCard> OnCardClicked;

    void Start() {
        gameObject.SetActive(false);
    }

    void OnEnable() {
        UpdateAvailability();
    }

    public void UpdateAvailability() {
        _unavailableOverlay.SetActive(!Available);
        Selectable.interactable = Available;
    }

    public IEnumerator RestockAnimation() {
        gameObject.SetActive(true);
        yield break;
    }

    public IEnumerator BuyAnimation(Transform target) {
        _layoutElement.ignoreLayout = true;
        LeanTween
            .value(gameObject, 0, 1, _buyTweenDuration)
            .setEase(_buyTweenType)
            .setOnUpdate(value => transform.position = Vector3.Slerp(transform.position, target.position, value));
        LeanTween.scale(gameObject, Vector3.zero, _buyTweenDuration).setEase(_buyTweenType);
        yield return CoroutineUtils.Wait(_buyTweenDuration);
        gameObject.SetActive(false);
        transform.localScale = Vector3.one;
        _layoutElement.ignoreLayout = false;
    }

    public new void OnPointerClick(PointerEventData e) {
        base.OnPointerClick(e);
        if (e.button != PointerEventData.InputButton.Left) return;
        OnCardClicked?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        UpdateAvailability();
    }
}