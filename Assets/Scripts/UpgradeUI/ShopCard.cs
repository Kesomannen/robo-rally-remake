using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopCard : UpgradeCard, IPointerClickHandler {
    [SerializeField] DynamicUITween _onEnableTween;
    
    public event Action<ShopCard> OnCardClicked;
    
    void Start() {
        StartCoroutine(TweenHelper.DoUITween(_onEnableTween, gameObject));
    }

    public void Remove() {
        Destroy(gameObject);
    }

    public void OnBuy() {
        Destroy(gameObject);
    }

    public new void OnPointerClick(PointerEventData e) {
        base.OnPointerClick(e);
        if (e.button == PointerEventData.InputButton.Left) {
            OnCardClicked?.Invoke(this);
        }
    }
}