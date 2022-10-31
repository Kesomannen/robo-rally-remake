using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Conveyor : StaticObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float _startProgress = 1f;

    public override bool CanEnter(Vector2Int direction) => true;

    static Dictionary<MapObject, float> _progress;

    MapObject _currentObject;

    static bool _isActive;
    static event Action _onActivate;

    protected override void Awake() {
        base.Awake();
        _direction = RotateVector(_direction);
    }

    public static IEnumerator Activate() {
        _progress = new();

        _isActive = true;
        _onActivate?.Invoke();

        yield return Scheduler.WaitUntilStackEmpty();

        _isActive = false;
    }

    public override void OnEnter(DynamicObject dynamic) {
        base.OnEnter(dynamic);
        if (_isActive) Convey();
        _onActivate += Convey;
    }

    public override void OnExit(DynamicObject dynamic) {
        base.OnExit(dynamic);
        _onActivate -= Convey;
    }

    void Convey() {
        if (_currentObject == null) return;

        var progress = _progress.EnforceKey(_currentObject, _startProgress);
        if (progress - _cost >= 0) {
            Scheduler.AddItem(ConveyorRoutine(DynamicObject));
        }
        
        IEnumerator ConveyorRoutine(DynamicObject dynamic) {
            var targetPos = GridPos + _direction;
            if (InteractionSystem.CanEnter(targetPos, -_direction)) {
                _progress[dynamic] -= _cost;
                yield return MapSystem.Instance.MoveObject(dynamic, targetPos);
            }
        }
    }
}