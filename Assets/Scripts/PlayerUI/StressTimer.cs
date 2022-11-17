using System;
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
        get{
            if (_isStressed){
                var stressTime = ProgrammingPhase.StressTimer.Value;
                var second = stressTime == 0 ? "second" : "seconds";
                return $"You have {stressTime} {second} left to finish your program!";
            } else{
                return "Nobody has finished their program yet.";
            }
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

    void OnStressValueChanged(int prev, int next){
        _text.text = next.ToString();
        if (!ProgrammingPhase.IsStressed) return;

        if (_isStressed){
            if (LeanTween.isTweening(_hourglass)) return;
            LeanTween.rotateAround(_hourglass, Vector3.forward, 180, 0.75f).setEase(LeanTweenType.easeSpring);
        } else{
            _isStressed = true;

            // Hourglass animation
            var source = _hourglass.transform.position.y;
            var target = source + _jumpHeight * CanvasUtils.Scale.y;

            LeanTween.moveY(_hourglass, target, _jumpDuration / 2).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => {
                LeanTween.moveY(_hourglass, source, _jumpDuration / 2).setEase(LeanTweenType.easeInExpo);
            });
            LeanTween.rotateAround(_hourglass, Vector3.forward, 180, _jumpDuration).setEase(LeanTweenType.easeInOutExpo);
        }
    }
}