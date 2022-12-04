using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MapObject : MonoBehaviour, IMapObject {
    public MapObject Object => this;
    public IRotator Rotator => _rotator;

    TransformRotator _rotator;

    #if UNITY_EDITOR
    [ReadOnly] 
    #endif
    public Vector2Int GridPos;

    protected virtual bool CanRotate => false;

    public event Action<int> OnRotationChanged;

    public virtual void Fall(IBoard board) {
        MapSystem.Instance.DestroyObject(this);
    }

    protected virtual void Awake(){
        _rotator = new TransformRotator(transform.eulerAngles);
    }

    public void RotateInstant(int steps) {
        if (steps == 0 || !CanRotate) return;

        _rotator.RotZ += steps;
        transform.eulerAngles = new Vector3(0, 0, _rotator.RotZ * 90f);
        OnRotationChanged?.Invoke(steps);
    }

    const float DefaultRotSpeed = 0.5f;
    const LeanTweenType DefaultRotEaseType = LeanTweenType.easeInOutSine;

    public IEnumerator RotateRoutine(int steps, float speed = DefaultRotSpeed, LeanTweenType easeType = DefaultRotEaseType) {
        if (steps == 0 || !CanRotate) yield break;
        
        _rotator.RotZ += steps;
        OnRotationChanged?.Invoke(steps);
        
        var duration = Math.Abs(steps) * speed;
        LeanTween.rotateZ(gameObject, _rotator.RotZ * 90f, duration).setEase(easeType);
        yield return CoroutineUtils.Wait(duration);
    }
}