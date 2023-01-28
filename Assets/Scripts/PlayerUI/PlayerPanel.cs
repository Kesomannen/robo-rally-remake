using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPanel : Container<Player>, IPointerClickHandler {
    [Header("Player Panel")]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _robotIcon;
    [SerializeField] OverlayData<PlayerOverlay> _overlayData;

    [Header("Indicators")]
    [SerializeField] UIPassiveAnimation _indicatorAnimator;
    [SerializeField] Sprite[] _doneAnim, _inProgressAnim, _waitingAnim;
    [SerializeField] Image _actionIndicator;
    [SerializeField] UpgradeCard _upgradeIndicator;
    [SerializeField] [Min(0)] float _actionIndicatorDuration;

    Player _player;
    bool _programLockedIn;

    protected override void Serialize(Player player) {
        if (_player != null) {
            _player.OnUpgradeUsed -= OnUpgradeUsed;
        }
        _player = player;
        _player.OnUpgradeUsed += OnUpgradeUsed;
        
        _nameText.text = player.ToString();
        _robotIcon.sprite = player.RobotData.Icon;
        
        ProgrammingPhase.OnPhaseStarted += OnPhaseStarted;
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerProgramDone;
        ProgrammingPhase.OnStressStarted += OnStressStarted;
        
        ShopPhase.OnPhaseStarted += OnPhaseStarted;
        ShopPhase.OnPlayerDecision += OnPlayerDecision;
        ShopPhase.OnNewPlayer += OnNewPlayer;

        OnPhaseStarted();
    }
    
    void OnUpgradeUsed(UpgradeCardData upgrade) {
        _upgradeIndicator.SetContent(upgrade);
        _actionIndicator.SetActive(true);
        
        LeanTween.delayedCall(_actionIndicatorDuration, () => {
            _actionIndicator.SetActive(false);
        });
    }

    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (_player != player) return;
        SetIndicatorState(IndicatorState.Done);
    }
    
    void OnNewPlayer(Player player) {
        if (_player != player) return;
        SetIndicatorState(IndicatorState.InProgress);
    }

    void OnPlayerProgramDone(Player player) {
        if (player != _player) return;
        _programLockedIn = true;
        SetIndicatorState(IndicatorState.Done);
    }

    void OnPhaseStarted() {
        _programLockedIn = false;
        SetIndicatorState(IndicatorState.Waiting);
    }
    
    void OnStressStarted(Player player) {
        if (_programLockedIn || player == _player) return;
        SetIndicatorState(IndicatorState.InProgress);
    }

    public void OnPointerClick(PointerEventData e) {
        OverlaySystem.Instance.PushAndShowOverlay(_overlayData).Init(_player);
    }
    
    void SetIndicatorState(IndicatorState state) {
        _indicatorAnimator._sprites = state switch {
            IndicatorState.Done => _doneAnim,
            IndicatorState.InProgress => _inProgressAnim,
            IndicatorState.Waiting => _waitingAnim,
            _ => _indicatorAnimator._sprites
        };
    }
    
    enum IndicatorState {
        Waiting,
        InProgress,
        Done
    }
}