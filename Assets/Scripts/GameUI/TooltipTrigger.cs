using System;
using UnityEngine;

public abstract class TooltipTrigger : MonoBehaviour {
    [SerializeField] TooltipableLocation _tooltipableLocation;

    protected ITooltipable Tooltipable;
    
    bool _isTooltipActive;

    protected virtual void Awake(){
        var item = GetTooltipable();
        if (item == null){
            Debug.LogError($"No tooltipable found on {gameObject.name}");
            return;
        }
        Tooltipable = item;
    }

    protected void OnDisable() {
        if (!_isTooltipActive) return;
        Tooltip.Instance.Hide(Tooltipable);
        _isTooltipActive = false;
    }

    ITooltipable GetTooltipable() {
        return _tooltipableLocation switch {
            TooltipableLocation.InSelf => GetComponent<ITooltipable>(),
            TooltipableLocation.InParent => GetComponentInParent<ITooltipable>(),
            TooltipableLocation.InChildren => GetComponentInChildren<ITooltipable>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected void Show() {
        Tooltip.Instance.Show(Tooltipable);
        _isTooltipActive = true;
    }
    
    protected void Hide() {
        Tooltip.Instance.Hide(Tooltipable);
        _isTooltipActive = false;
    }

    enum TooltipableLocation {
        InSelf,
        InChildren,
        InParent
    }
}