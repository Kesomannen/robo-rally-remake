using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MapObject : MonoBehaviour, IMapObject {
    public MapObject Object => this;
    public IRotator Rotator => _rotator;

    TransformRotator _rotator;

    [ReadOnly] public Vector2Int GridPos;

    public event Action<int> OnRotationChanged;

    public virtual void Fall(IBoard board) {
        MapSystem.Instance.DestroyObject(this);
    }

    protected virtual void Awake() {
        _rotator = TransformRotator.GetRotator(transform.eulerAngles);
    }

    public void RotateInstant(int steps) {
        if (steps == 0) return;

        _rotator.RotZ += steps;
        transform.eulerAngles = new Vector3(0, 0, _rotator.RotZ * 90f);
        OnRotationChanged?.Invoke(steps);
    }

    const float RotDuration = 0.5f;
    const LeanTweenType RotEaseType = LeanTweenType.easeInOutSine;

    public IEnumerator RotateRoutine(int steps) {
        if (steps == 0) yield break;
        
        _rotator.RotZ += steps;
        OnRotationChanged?.Invoke(steps);
        
        var duration = Math.Abs(steps) * RotDuration;
        LeanTween.rotateZ(gameObject, _rotator.RotZ * 90f, duration).setEase(RotEaseType);
        yield return Helpers.Wait(duration);
    }
}