using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Conveyor : BoardElement<Conveyor> {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float _startProgress = 1f;

    static Dictionary<MapObject, float> _progress;

    static bool _isActive;

    protected override void Awake() {
        base.Awake();
        _direction = RotateAsObject(_direction);
    }

    public static new IEnumerator ActivateRoutine() {
        _progress = new();

        _isActive = true;
        _onActivate?.Invoke();

        yield return Scheduler.WaitUntilClearRoutine();

        _isActive = false;
    }

    public override void OnEnter(DynamicObject dynamic) {
        base.OnEnter(dynamic);
        if (_isActive) OnActivate();
    }

    protected override void OnActivate() {
        var progress = _progress.EnforceKey(CurrentDynamic, _startProgress);
        if (progress - _cost >= 0 && Interaction.SoftMove(CurrentDynamic, _direction, out var routine)) {
            _progress[CurrentDynamic] -= _cost;
            Scheduler.Push(routine);
        }
    }
}