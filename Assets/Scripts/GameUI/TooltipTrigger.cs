using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] TooltipableLocation _tooltipableLocation;

    ITooltipable _item;

    void Awake() {
        _item = GetTooltipable();
    }

    ITooltipable GetTooltipable() {
        return _tooltipableLocation switch {
            TooltipableLocation.InSelf => GetComponent<ITooltipable>(),
            TooltipableLocation.InParent => GetComponentInParent<ITooltipable>(),
            TooltipableLocation.InChildren => GetComponentInChildren<ITooltipable>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void OnPointerEnter(PointerEventData _) {
        Tooltip.Instance.Show(_item);
    }
    
    public void OnPointerExit(PointerEventData _) {
        Tooltip.Instance.Hide();
    }

    enum TooltipableLocation {
        InSelf,
        InChildren,
        InParent
    }
}