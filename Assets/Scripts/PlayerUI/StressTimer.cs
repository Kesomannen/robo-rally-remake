using TMPro;
using UnityEngine;

public class StressTimer : MonoBehaviour, ITooltipable {
    [SerializeField] GameObject _hourglass;
    [SerializeField] float _jumpHeight;
    [SerializeField] float _jumpDuration;
    [SerializeField] TMP_Text _text;

    bool _isStressed;

    public string Header => "Programming Timer";
    public string Description {
        get {
            return !_isStressed 
                ? "Nobody has finished their program yet." 
                : $"You have {StringUtils.FormatMultiple(ProgrammingPhase.StressTimer.Value, "second")} left to finish your program!";
        }
    }
    
    void Awake() {
        ProgrammingPhase.StressTimer.OnValueChanged += OnStressValueChanged;
        ProgrammingPhase.OnPhaseStarted += OnPhaseStarted;
    }

    void OnDestroy(){
        ProgrammingPhase.StressTimer.OnValueChanged -= OnStressValueChanged;
        ProgrammingPhase.OnPhaseStarted -= OnPhaseStarted;
    }
    
    void OnPhaseStarted() {
        _isStressed = false;
        _text.text = "---";
    }

    void OnStressValueChanged(int prev, int next) {
        if (!ProgrammingPhase.IsStressed) {
            _text.text = "---";
            return;
        }
        _text.text = next.ToString();

        if (_isStressed) {
            if (LeanTween.isTweening(_hourglass)) return;
            LeanTween.rotateAround(_hourglass, Vector3.forward, 180, 0.75f).setEase(LeanTweenType.easeSpring);
        } else {
            _isStressed = true;

            // Jump animation
            var source = _hourglass.transform.position.y;
            var target = source + _jumpHeight * CanvasUtils.CanvasScale.y;

            LeanTween.moveY(_hourglass, target, _jumpDuration / 2).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => {
                LeanTween.moveY(_hourglass, source, _jumpDuration / 2).setEase(LeanTweenType.easeInExpo);
            });
            LeanTween.rotateAround(_hourglass, Vector3.forward, 180, _jumpDuration).setEase(LeanTweenType.easeInOutExpo);
        }
    }
}