using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ShopCard : UpgradeCard, IPointerClickHandler {
    [FormerlySerializedAs("_onEnableTween")]
    [SerializeField] DynamicUITween _onRestockTween;
    [FormerlySerializedAs("_onDisappearTween")]
    [SerializeField] DynamicUITween _onBoughtTween;
    [FormerlySerializedAs("_onEnableParticles")] 
    [SerializeField] ParticleSystem _onRestockParticles;
    [SerializeField] GameObject _hiddenOverlay;

    bool _hidden;

    public event Action<ShopCard> OnCardClicked;

    void Awake() {
        Hide();
    }

    protected override void Serialize(UpgradeCardData card) {
        base.Serialize(card);
        _hiddenOverlay.SetActive(false);
        _hidden = false;
    }

    void Hide() {
        _hiddenOverlay.SetActive(true);
        _hidden = true;
    }

    public IEnumerator RestockAnimation() {
        _onRestockParticles.Play();
        yield return TweenHelper.DoUITween(_onRestockTween, gameObject);
    }

    public IEnumerator BuyAnimation() {
        yield return TweenHelper.DoUITween(_onBoughtTween, gameObject);
        Hide();
    }

    public new void OnPointerClick(PointerEventData e) {
        base.OnPointerClick(e);
        if (e.button == PointerEventData.InputButton.Left && !_hidden) {
            OnCardClicked?.Invoke(this);
        }
    }
}