using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPanel : Container<Player>, IPointerClickHandler {
    [Header("Player Panel")]
    [SerializeField] Optional<TMP_Text> _nameText;
    [SerializeField] Optional<Image> _robotIcon;
    [SerializeField] Optional<TMP_Text> _energyText;
    [SerializeField] bool _showOverlayOnClick;
    
    [ShowIf("_showOverlayOnClick")]
    [SerializeField] OverlayData<PlayerOverlay> _overlayData;

    [Header("Indicator")]
    [SerializeField] UIPassiveAnimation _indicatorAnimator;
    [SerializeField] Sprite[] _doneAnim, _inProgressAnim, _waitingAnim;

    [Header("Tween")]
    [ShowIf("_energyText.Enabled")]
    [SerializeField] Image _energyIcon;
    [SerializeField] [Min(0)] float _tweenDuration;
    [SerializeField] LeanTweenType _tweenType;

    static bool EnergyEnabled => PlayerSystem.EnergyEnabled;
    
    void Start() {
        if (EnergyEnabled || !_energyText.Enabled) return;
        _energyText.Value.gameObject.SetActive(false);
        _energyIcon.gameObject.SetActive(false);
    }

    protected override void Serialize(Player player) {
        if (_nameText.Enabled) _nameText.Value.text = PlayerSystem.IsLocal(player) ? $"{player} (You)" : player.ToString();
        if (_energyText.Enabled && EnergyEnabled) _energyText.Value.text = player.Energy.ToString();
        Content.Energy.ValueChanged += OnEnergyChanged;
        if (_robotIcon.Enabled) _robotIcon.Value.sprite = player.RobotData.Icon;
    }
    
    void OnEnergyChanged(int prev, int next) {
        if (!_energyText.Enabled || !EnergyEnabled) return;
        var energyText = _energyText.Value;

        var effectColor = next > prev ? Color.green : Color.red;
        LeanTween
            .value(energyText.gameObject, effectColor, energyText.color, _tweenDuration)
            .setEase(_tweenType)
            .setOnUpdate(c => {
                energyText.color = c;
                _energyIcon.color = c;
            });
        
        energyText.text = next.ToString();
    }

    public void OnPointerClick(PointerEventData e) {
        if (!_showOverlayOnClick) return;
        OverlaySystem.Instance.PushAndShowOverlay(_overlayData).Init(Content);
    }
    
    protected void SetIndicatorState(IndicatorState state) {
        _indicatorAnimator._sprites = state switch {
            IndicatorState.Done => _doneAnim,
            IndicatorState.InProgress => _inProgressAnim,
            IndicatorState.Waiting => _waitingAnim,
            _ => _indicatorAnimator._sprites
        };
    }
    
    protected enum IndicatorState {
        Waiting,
        InProgress,
        Done
    }
}