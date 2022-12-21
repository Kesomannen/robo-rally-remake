using System;
using UnityEngine;
using UnityEngine.UI;

public class RegisterBar : MonoBehaviour {
    [SerializeField] Image _indicator;
    [SerializeField] Sprite[] _indicatorSprites;
    [SerializeField] RectTransform[] _slots;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenTime;

    void Awake() {
        ExecutionPhase.OnNewRegister += MoveIndicatorTo;
    }

    void OnDestroy() {
        ExecutionPhase.OnNewRegister -= MoveIndicatorTo;
    }

    void OnEnable() {
        MoveIndicatorTo(0);
    }

    int _tweenId;
    
    void MoveIndicatorTo(int slotIndex) {
        LeanTween.cancel(_tweenId);
        _tweenId = LeanTween
            .move(_indicator.gameObject, _slots[slotIndex].position, _tweenTime)
            .setEase(_tweenType)
            .uniqueId;
        LeanTween.delayedCall(_tweenTime / 2, () => _indicator.sprite = _indicatorSprites[slotIndex]);
    }
}