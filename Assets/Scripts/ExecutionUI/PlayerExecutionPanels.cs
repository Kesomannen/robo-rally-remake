using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExecutionPanels : MonoBehaviour {
    [SerializeField] PlayerExecutionPanel _panelPrefab;
    [SerializeField] int _panelSpacing;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenTime;
    
    readonly List<PlayerExecutionPanel> _panels = new();
    
    public IReadOnlyList<PlayerExecutionPanel> Panels => _panels;

    public IEnumerator Swap(Player first, Player second) {
        yield return Swap(FindIndex(first), FindIndex(second));
        int FindIndex(Player player) => _panels.FindIndex(p => p.Content == player);
    }
    
    public IEnumerator Swap(int first, int second) {
        if (first == second) yield break;
        
        var firstPanel = _panels[first];
        var secondPanel = _panels[second];

        _panels[first] = secondPanel;
        _panels[second] = firstPanel;

        yield return this.RunRoutines(
            LerpPanel(firstPanel, GetYPosition(second)),
            LerpPanel(secondPanel, GetYPosition(first))
        );
    }
    
    public void CreatePanel(Player player) {
        var panel = Instantiate(_panelPrefab, transform);
        panel.transform.localPosition = new Vector3(0, GetYPosition(_panels.Count));
        
        panel.SetContent(player);
        _panels.Add(panel);
    }

    int GetYPosition(int index) => -_panelSpacing * index;
    
    IEnumerator LerpPanel(PlayerExecutionPanel panel, int targetY) {
        var current = (int)panel.transform.localPosition.y;
        var distance = Mathf.Abs(targetY - current);

        LeanTween.cancel(panel.gameObject);
        var duration = _tweenTime * distance / _panelSpacing;
        LeanTween.moveLocalY(panel.gameObject, targetY, duration).setEase(_tweenType);
        yield return CoroutineUtils.Wait(duration);
    }
}