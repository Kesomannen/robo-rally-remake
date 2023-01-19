using System;
using UnityEngine;

public abstract class TooltipTrigger : MonoBehaviour {
    [SerializeField] TooltipableLocation _tooltipableLocation;

    ITooltipable _tooltipable;
    bool _isTooltipActive;

    protected virtual void Awake() {
        var item = GetTooltipable();
        if (item == null){
            Debug.LogError($"No tooltipable found on {gameObject.name}");
            return;
        }
        _tooltipable = item;
    }

    protected void OnDisable() {
        if (!_isTooltipActive) return;
        Tooltip.Instance.Hide(_tooltipable);
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
        Tooltip.Instance.Show(_tooltipable);
        _isTooltipActive = true;
    }
    
    protected void Hide() {
        Tooltip.Instance.Hide(_tooltipable);
        _isTooltipActive = false;
    }

    enum TooltipableLocation {
        InSelf,
        InChildren,
        InParent
    }
}