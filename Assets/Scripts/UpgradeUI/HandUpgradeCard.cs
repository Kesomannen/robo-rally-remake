using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Container<UpgradeCardData>))]
public class HandUpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenDuration;
    [SerializeField] float _highlightScale;
    [SerializeField] GameObject _unavailableOverlay;
    [SerializeField] Selectable _selectable;

    Container<UpgradeCardData> _container;
    Transform _highlightParent;
    Transform _originalParent;
    Vector3 _originalSize;

    static Player Owner => PlayerSystem.LocalPlayer;

    public Transform HighlightParent { set => _highlightParent = value; }
    
    void OnDestroy() {
        ProgrammingPhase.OnPlayerLockedIn -= OnPlayerLockedIn;
        ProgrammingPhase.OnPhaseStarted -= UpdateAvailability;
    }
    
    void Awake() {
        _container = GetComponent<Container<UpgradeCardData>>();
        
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerLockedIn;
        ProgrammingPhase.OnPhaseStarted += UpdateAvailability;
        
        var t = transform;
        _originalParent = t.parent;
        _originalSize = t.localScale;
    }

    void Start() {
        UpdateAvailability();
    }

    void OnPlayerLockedIn(Player player) => UpdateAvailability();
    
    void UpdateAvailability() {
        var available = _container.Content != null 
            && _container.Content.CanUse(PlayerSystem.LocalPlayer) 
            || _container.Content.Type == UpgradeType.Permanent;
        
        _unavailableOverlay.gameObject.SetActive(!available);
        _selectable.interactable = available;
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
        UpdateAvailability();
        if (eventData.button != PointerEventData.InputButton.Left) return;
        TaskScheduler.PushRoutine(UseUpgrade());

        IEnumerator UseUpgrade() {
            var upgrade = _container.Content;
            if (!upgrade.CanUse(Owner)) yield break;
            Owner.Energy.Value -= upgrade.UseCost;
        
            var index = Owner.Upgrades.IndexOf(upgrade);
            Owner.UseUpgrade(index);
            NetworkSystem.Instance.BroadcastUpgrade(index);
        }
    }
}