using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Transition : MonoBehaviour {
    [SerializeField] Animator _animator;
    [SerializeField] string _triggerName;
    [SerializeField] TMP_Text _text;
    
    Action _onMiddle;
    bool _isPlaying;

    public IEnumerator DoTransition(string text, Action onMiddle) {
        gameObject.SetActive(true);
        _isPlaying = true;
        
        if (!string.IsNullOrEmpty(text)) {
            _text.text = text;
        }
        
        _animator.SetTrigger(_triggerName);
        _onMiddle = onMiddle;

        yield return new WaitUntil(() => _isPlaying);
    }
    
    public void OnTransitionEnd() {
        gameObject.SetActive(false);
        _isPlaying = false;
    }
    
    public void OnTransitionMiddle() {
        _onMiddle?.Invoke();
    }
}