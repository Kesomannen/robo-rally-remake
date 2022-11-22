using System;
using UnityEngine;

public abstract class TooltipTrigger : MonoBehaviour {
    [SerializeField] TooltipableLocation _tooltipableLocation;

    protected ITooltipable Tooltipable;

    protected virtual void Awake(){
        var item = GetTooltipable();
        if (item == null){
            Debug.LogError($"No tooltipable found on {gameObject.name}");
            return;
        }
        Tooltipable = item;
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
    }
    
    protected void Hide() {
        Tooltip.Instance.Hide(Tooltipable);
    }

    enum TooltipableLocation {
        InSelf,
        InChildren,
        InParent
    }
}