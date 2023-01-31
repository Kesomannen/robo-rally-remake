using System.Collections.Generic;
using UnityEngine;

public class PlayerExecutionPanels : MonoBehaviour {
    [SerializeField] PlayerExecutionPanel _panelPrefab;
    [SerializeField] float _panelSpacing;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenTime;
    
    readonly List<PlayerExecutionPanel> _panels = new();

    void Awake() {
        _panelSpacing *= CanvasUtils.CanvasScale.x;
    }

    public void CreatePanel(Player player) {
        var panel = Instantiate(_panelPrefab, transform);
        panel.SetContent(player);
        _panels.Add(panel);
        
        UpdatePosition();
    }

    void UpdatePosition() {
        for (var i = 0; i < _panels.Count; i++) {
            LerpTo(_panels[i], _panelSpacing * i);            
        }
    }

    void LerpTo(PlayerExecutionPanel panel, float targetY) {
        LeanTween.cancel(panel.gameObject);
        var duration = _tweenTime * Mathf.Abs(targetY - panel.transform.localPosition.y) / _panelSpacing;
        LeanTween.moveLocalY(panel.gameObject, targetY, duration).setEase(_tweenType);
    }
}