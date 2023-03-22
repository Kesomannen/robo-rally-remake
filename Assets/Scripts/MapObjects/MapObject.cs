using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MapObject : MonoBehaviour, IMapObject {
    public MapObject Object => this;
    public TransformRotator Rotator { get; private set; }
    
    // ReSharper disable once InconsistentNaming
    [ReadOnly] public Vector2Int GridPos;

    public virtual bool CanRotate => false;

    public event Action<int> RotationChanged;

    public virtual void Fall(IBoard board) {
        MapSystem.DestroyObject(this);
    }

    protected virtual void Awake(){
        Rotator = new TransformRotator(transform.eulerAngles);
    }

    public void RotateInstant(int steps) {
        if (steps == 0 || !CanRotate) return;

        Rotator.RotZ += steps;
        transform.eulerAngles = new Vector3(0, 0, Rotator.RotZ * 90f);
        RotationChanged?.Invoke(steps);
    }

    const float DefaultRotSpeed = 2f;
    const LeanTweenType DefaultRotEaseType = LeanTweenType.easeInOutSine;

    public IEnumerator RotateRoutine(int steps, float speed = DefaultRotSpeed, LeanTweenType easeType = DefaultRotEaseType) {
        if (steps == 0 || !CanRotate) yield break;
        
        Rotator.RotZ += steps;
        RotationChanged?.Invoke(steps);
        
        var duration = Math.Abs(steps) / speed;
        LeanTween.rotateZ(gameObject, Rotator.RotZ * 90f, duration).setEase(easeType);
        yield return CoroutineUtils.Wait(duration);
    }
}