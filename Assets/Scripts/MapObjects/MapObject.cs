using System;
using System.Collections;
using UnityEngine;

public abstract class MapObject : MonoBehaviour, IMapObject {
    public MapObject Object => this;
    public IRotator Rotator => _rotator;

    TransformRotator _rotator;

    public Vector2Int GridPos { get; set; }

    public event Action<int> OnRotationChanged;
    public event Action<Vector2Int, Vector2Int> OnPositionChanged;

    protected virtual void Awake() {
        _rotator = TransformRotator.GetRotator(transform.eulerAngles);
    }

    public void RotateInstant(int steps) {
        if (steps == 0) return;

        _rotator.RotZ += steps;
        var target = _rotator.RotZ * 90f;
        transform.eulerAngles = new(0, 0, target);
        OnRotationChanged?.Invoke(steps);
    }

    const float _rotDuration = 0.5f;
    const LeanTweenType _rotEaseType = LeanTweenType.easeInOutSine;

    public IEnumerator RotateRoutine(int steps) {
        if (steps == 0) yield break;

        var iterations = Mathf.Abs(steps);
        var delta = steps / iterations;

        for (int i = 0; i < iterations; i++) {
            _rotator.RotZ += delta;
            var target = _rotator.RotZ * 90f;

            OnRotationChanged?.Invoke(delta);

            yield return LeanTween.rotateZ(gameObject, target, _rotDuration).setEase(_rotEaseType);
        }
    }
}