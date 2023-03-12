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
    [Space]
    [SerializeField] [ReadOnly] bool _usedThisTurn;
    [SerializeField] [ReadOnly] Vector3 _originalSize;
    [SerializeField] [ReadOnly] bool _clickable = true;
    
    Container<UpgradeCardData> _container;
    Transform _highlightParent;
    Transform _originalParent;

    static Player Owner => PlayerSystem.LocalPlayer;
    UpgradeCardData Content => _container.Content;
    
    public Transform HighlightParent { set => _highlightParent = value; }
    public bool Available => _selectable.interactable;
    
    public static event Action<HandUpgradeCard> OnCardClicked; 

    public void Awake() {
        var t = transform;
        _originalParent = t.parent;
        _originalSize = t.localScale;
        
        ProgrammingPhase.OnPhaseStarted += OnProgrammingStarted;
    }

    void OnDestroy() {
        ProgrammingPhase.OnPhaseStarted -= OnProgrammingStarted;
    }

    void Start() {
        UpdateAvailability();
    }
    
    void OnProgrammingStarted() {
        _usedThisTurn = false;
    }

    public void UpdateAvailability() {
        if (_container == null) _container = GetComponent<Container<UpgradeCardData>>();
        
        var available = Content != null
                        && !_usedThisTurn
                        && Owner.Energy.Value >= Content.UseCost
                        && (Content.CanUse(PlayerSystem.LocalPlayer)
                            || Content.Type == UpgradeType.Permanent);
        
        _unavailableOverlay.gameObject.SetActive(!available);
        _selectable.interactable = available;
    }

    void SetHighlighted(bool highlighted) {
        var t = transform;
        if (highlighted) {
            t.SetParent(_highlightParent, true);
            t.SetAsLastSibling();

            ScaleTo(_highlightScale);
        } else {
            t.SetParent(_originalParent);
            
            ScaleTo(1);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData) => SetHighlighted(true);
    public void OnPointerExit(PointerEventData eventData) => SetHighlighted(false);

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
        
        if (!_clickable || !_selectable.interactable) return;
        if (Content.Type == UpgradeType.Permanent) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        _clickable = false;
        OnCardClicked?.Invoke(this);
        TaskScheduler.PushRoutine(UseUpgrade());

        IEnumerator UseUpgrade() {
            var index = Owner.Upgrades.IndexOf(Content);
            
            NetworkSystem.Instance.BroadcastUpgrade(index);
            Owner.Energy.Value -= Content.UseCost;
            Owner.UseUpgrade(index);
            
            _usedThisTurn = true;
            _clickable = true;

            yield break;
        }
    }
}