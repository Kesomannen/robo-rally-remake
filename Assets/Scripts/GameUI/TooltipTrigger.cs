using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TooltipTrigger : MonoBehaviour {
    [SerializeField] TooltipableLocation _tooltipableLocation;

    ITooltipable _item;

    protected virtual void Awake(){
        var item = GetTooltipable();
        if (item == null){
            Debug.LogError($"No tooltipable found on {gameObject.name}");
            return;
        }
        _item = item;
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
        Tooltip.Instance.Show(_item);
    }
    
    protected void Hide() {
        Tooltip.Instance.Hide();
    }

    enum TooltipableLocation {
        InSelf,
        InChildren,
        InParent
    }
}