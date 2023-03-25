using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopCard : UpgradeCard, IPointerClickHandler, IPointerEnterHandler {
    [SerializeField] LayoutElement _layoutElement;
    [SerializeField] GameObject _unavailableOverlay;
    [SerializeField] ParticleSystem _restockParticle;
    [Space]
    [SerializeField] float _buyTweenDuration = 0.5f;
    [SerializeField] LeanTweenType _buyTweenType;
    [Space]
    [SerializeField] float _restockTweenDuration = 0.5f;
    [SerializeField] LeanTweenType _restockTweenType;

    public bool Available {
        get {
            var localPlayer = PlayerSystem.LocalPlayer;

            return gameObject.activeInHierarchy
                   && ShopPhase.Instance.CurrentPlayer == localPlayer
                   && Content != null
                   && Content.Cost <= localPlayer.Energy.Value
                   && !localPlayer.Upgrades.Contains(Content);
        }
    }

    public event Action<ShopCard> CardClicked;

    public void UpdateAvailability() {
        _unavailableOverlay.SetActive(!Available);
        Selectable.interactable = Available;
    }

    public IEnumerator RestockAnimation(UpgradeCardData card) {
        var t = transform;

        if (gameObject.activeInHierarchy) {
            LeanTween.scale(gameObject, Vector3.zero, _restockTweenDuration / 2).setEase(_buyTweenType);
            yield return CoroutineUtils.Wait(_restockTweenDuration);   
        }

        t.localScale = Vector3.zero;
        //t.rotation = Quaternion.Euler(0, 0, 180);
        gameObject.SetActive(true);
        SetContent(card);   
        
        LeanTween.scale(gameObject, Vector3.one, _restockTweenDuration).setEase(_restockTweenType);
        LeanTween.rotateZ(gameObject, 0, _restockTweenDuration).setEase(_restockTweenType);
        yield return CoroutineUtils.Wait(_restockTweenDuration / 2);
        _restockParticle.Play();
        yield return CoroutineUtils.Wait(_restockTweenDuration / 2);
    }

    public IEnumerator BuyAnimation(Transform target) {
        var t = transform;
        _layoutElement.ignoreLayout = true;

        LeanTween.move(gameObject, target.position, _buyTweenDuration).setEase(_buyTweenType);
        LeanTween.scale(gameObject, Vector3.zero, _buyTweenDuration).setEase(_buyTweenType);
        LeanTween.rotateZ(gameObject, 180, _buyTweenDuration).setEase(_buyTweenType);
        yield return CoroutineUtils.Wait(_buyTweenDuration);
        
        gameObject.SetActive(false);

        t.localScale = Vector3.one;
        t.rotation = Quaternion.identity;
        _layoutElement.ignoreLayout = false;
    }

    public new void OnPointerClick(PointerEventData e) {
        base.OnPointerClick(e);
        if (e.button != PointerEventData.InputButton.Left) return;
        CardClicked?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        UpdateAvailability();
    }
}