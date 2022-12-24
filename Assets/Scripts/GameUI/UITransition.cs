using System;
using TMPro;
using UnityEngine;

public class UITransition : MonoBehaviour {
    [SerializeField] Animator _animator;
    [SerializeField] string _triggerName;
    [SerializeField] TMP_Text _text;
    
    Action _onMiddle;

    public void DoTransition(string text, Action onMiddle) {
        gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(text)) _text.text = text;
        _animator.SetTrigger(_triggerName);
        _onMiddle = onMiddle;
    }
    
    public void OnTransitionEnd() {
        gameObject.SetActive(false);
    }
    
    public void OnTransitionMiddle() {
        _onMiddle?.Invoke();
    }
}