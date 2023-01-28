using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopCard : UpgradeCard, IPointerClickHandler {
    [SerializeField] DynamicUITween _onEnableTween;
    [SerializeField] DynamicUITween _onDisappearTween;
    [SerializeField] ParticleSystem _onEnableParticles;
    
    public event Action<ShopCard> OnCardClicked;

    public IEnumerator RestockAnimation() {
        _onEnableParticles.Play();
        yield return TweenHelper.DoUITween(_onEnableTween, gameObject);
    }

    public IEnumerator DisappearAnimation() {
        yield return TweenHelper.DoUITween(_onDisappearTween, gameObject);
        Destroy(gameObject);
    }

    public IEnumerator BuyAnimation() {
        transform.localScale = Vector3.zero;
        yield return TweenHelper.DoUITween(_onDisappearTween, gameObject);
        Destroy(gameObject);
    }

    public new void OnPointerClick(PointerEventData e) {
        base.OnPointerClick(e);
        if (e.button == PointerEventData.InputButton.Left) {
            OnCardClicked?.Invoke(this);
        }
    }
}