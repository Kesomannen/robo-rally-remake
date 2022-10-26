using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Conveyor : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float startProgress = 1f;

    public override bool IsStatic => true;
    public override bool CanEnter(Vector2Int direction) => true;

    static Dictionary<MapObject, float> _progress = new();
    static bool _hasListened;

    void Awake() {
        var rotSteps = Mathf.RoundToInt((transform.eulerAngles.z / 90) % 4);
        _direction = _direction.RotateCW(rotSteps);
    }

    void Start() {
        if (!_hasListened) {
            _hasListened = true;
            TurnSystem.instance.OnStepStart.AddListener(() => {
                _progress = new();
            });
        }
    }

    public override void OnEnter(MapObject dynamic) {
        Scheduler.AddItem(ConveyorRoutine(dynamic));
    }

    IEnumerator ConveyorRoutine(MapObject dynamic) {
        var progress = _progress.EnforceKey(dynamic, startProgress);
        if (progress - _cost < 0) {
            yield break;
        }

        var targetPos = GetGridPos() + _direction;
        if (InteractionSystem.CanEnter(targetPos, -_direction)) {
            yield return MapSystem.instance.MoveMapObject(dynamic, targetPos);
            _progress[dynamic] -= _cost;
            yield return new WaitForSeconds(1f);
        }
    }
}