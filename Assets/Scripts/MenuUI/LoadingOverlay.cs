using System;
using TMPro;
using UnityEngine;

public class LoadingOverlay : Singleton<LoadingOverlay> {
    [SerializeField] TMP_Text _messageText;

    protected override void Awake() {
        base.Awake();
        gameObject.SetActive(false);
    }

    public void Show(string message) {
        gameObject.SetActive(true);
        _messageText.text = message;
    }
    
    public void Hide() {
        gameObject.SetActive(false);
    }
}

public struct LoadScreen : IDisposable {
    public LoadScreen(string message) {
        LoadingOverlay.Instance.Show(message);
    }
    
    public void Dispose() {
        LoadingOverlay.Instance.Hide();    
    }
}