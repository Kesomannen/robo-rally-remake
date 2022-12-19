using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] Container<UpgradeCardData> _upgradeCard;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenDuration;
    [SerializeField] float _highlightScale;
    
    Vector3 _originalSize;

    static Player Owner => PlayerManager.LocalPlayer;

    void Awake() {
        _originalSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        ScaleTo(_highlightScale);
    }
    
    public void OnPointerExit(PointerEventData eventData) {
        ScaleTo(1);
    }

    int _tweenId;
    
    void ScaleTo(float scale) {
        if (_tweenId != 0) LeanTween.cancel(_tweenId);
        _tweenId = LeanTween
            .scale(gameObject, _originalSize * scale, _tweenDuration)
            .setEase(_tweenType)
            .uniqueId;
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        var upgrade = _upgradeCard.Content;
        if (!upgrade.CanUse(Owner)) return;
        Owner.UseUpgrade(Owner.Upgrades.IndexOf(upgrade));
    }
}