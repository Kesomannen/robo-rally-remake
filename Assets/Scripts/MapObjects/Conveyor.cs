using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Conveyor : StaticObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float _startProgress = 1f;

    public override bool CanEnter(Vector2Int direction) => true;
    public override bool CanExit(Vector2Int direction) => true;

    static Dictionary<MapObject, float> _progress;

    static bool _isActive;
    static event Action _onActivate;

    protected override void Awake() {
        base.Awake();
        _direction = RotateVector(_direction);
    }

    public static IEnumerator ActivateRoutine() {
        _progress = new();

        _isActive = true;
        _onActivate?.Invoke();

        yield return Scheduler.WaitUntilClearRoutine();

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
        if (CurrentDynamic == null) return;

        var progress = _progress.EnforceKey(CurrentDynamic, _startProgress);
        if (progress - _cost >= 0 && Interaction.SoftMove(CurrentDynamic, _direction, out var routine)) {
            _progress[CurrentDynamic] -= _cost;
            Scheduler.AddRoutine(routine);
        }
    }
}