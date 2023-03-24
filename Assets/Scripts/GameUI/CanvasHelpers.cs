using System;
using TMPro;
using UnityEngine;

public class CanvasHelpers : Singleton<CanvasHelpers> {
    [SerializeField] TMP_Text _messageText;
    [SerializeField] GameObject _overlay;
    [Space]
    [SerializeField] TMP_Text _errorText;
    [SerializeField] GameObject _errorObject;
    [Space]
    [SerializeField] SoundEffect _errorSound;

    void OnEnable() {
        MenuSystem.OnMenuChanged += OnMenuChanged;
    }
    
    void OnDisable() {
        MenuSystem.OnMenuChanged -= OnMenuChanged;
    }
    
    void OnMenuChanged() {
        _errorObject.SetActive(false);
        LeanTween.cancel(_errorTweenId);
    }

    public void ShowOverlay(string message) {
        _overlay.SetActive(true);
        _messageText.text = message;
    }

    public void SetOverlayText(string text) {
        _messageText.text = text;
    }
    
    public void HideOverlay() {
        _overlay.SetActive(false);
    }

    int _errorTweenId;
    
    public void ShowError(string message) {
        _errorSound.Play();
        _errorObject.SetActive(true);
        _errorText.text = message;
        
        LeanTween.cancel(_errorTweenId);
        _errorTweenId = LeanTween
            .delayedCall(4f, () => _errorObject.SetActive(false))
            .uniqueId;
    }
}

public struct LoadingScreen : IDisposable {
    public LoadingScreen(string message) {
        CanvasHelpers.Instance.ShowOverlay(message);
    }
    
    public void Dispose() {
        CanvasHelpers.Instance.HideOverlay();    
    }
}