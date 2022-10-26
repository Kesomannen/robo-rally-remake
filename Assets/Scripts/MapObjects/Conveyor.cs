using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Conveyor : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float _startProgress = 1f;

    public override bool IsStatic => true;
    public override bool CanEnter(Vector2Int direction) => true;

    static Dictionary<MapObject, float> _progress = new();

    MapObject _currentObject;
    bool _isActive;

    void Awake() {
        _direction = _direction.RotateAsTransform(transform);
        TurnSystem.OnInstanceCreated += _ => {
            _.OnStepStart += OnStepStart;
            _.OnStepEnd += OnStepEnd;
        };
    }

    void OnStepStart(int step) {
        _isActive = false;
    }

    void OnStepEnd(int step) {
        _progress.Clear();
        _isActive = true;
        if (_currentObject != null) {
            Convey();
        }
    }

    public override void OnEnter(MapObject dynamic) {
        _currentObject = dynamic;
        if (_isActive) Convey();
    }

    public override void OnExit(MapObject dynamic) {
        _currentObject = null;
    }

    void Convey() {
        var progress = _progress.EnforceKey(_currentObject, _startProgress);
        if (progress - _cost >= 0) {
            Scheduler.AddItem(new ScheduleRoutine(ConveyorRoutine(_currentObject)));
        }
        
        IEnumerator ConveyorRoutine(MapObject dynamic) {
            var targetPos = GetGridPos() + _direction;
            if (InteractionSystem.CanEnter(targetPos, -_direction)) {
                _progress[dynamic] -= _cost;
                yield return MapSystem.instance.MoveMapObject(dynamic, targetPos);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}