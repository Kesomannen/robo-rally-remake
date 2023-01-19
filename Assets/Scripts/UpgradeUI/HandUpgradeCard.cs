using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Container<UpgradeCardData>))]
public class HandUpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenDuration;
    [SerializeField] float _highlightScale;

    Container<UpgradeCardData> _container;
    Transform _highlightParent;
    Transform _originalParent;
    Vector3 _originalSize;

    static Player Owner => PlayerSystem.LocalPlayer;

    public Transform HighlightParent { set => _highlightParent = value; }
    
    void Awake() {
        _container = GetComponent<Container<UpgradeCardData>>();
        
        var t = transform;
        _originalParent = t.parent;
        _originalSize = t.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        var t = transform;
        t.SetParent(_highlightParent, true);
        t.SetAsLastSibling();
        
        ScaleTo(_highlightScale);
    }
    
    public void OnPointerExit(PointerEventData eventData) {
        transform.SetParent(_originalParent);
        
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
        
        var upgrade = _container.Content;
        if (!upgrade.CanUse(Owner)) return;
        Owner.Energy.Value -= upgrade.UseCost;
        Owner.UseUpgrade(Owner.Upgrades.IndexOf(upgrade));
    }
}