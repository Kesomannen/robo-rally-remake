using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Conveyor : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float startProgress = 1f;

    public override bool IsStatic => true;
    public override bool CanEnter(Vector2Int direction) => true;

    static Dictionary<MapObject, float> _progress;
    static bool _hasListened;

    void Awake() {
        var rotSteps = Mathf.RoundToInt((transform.eulerAngles.z / 90) % 4);
        _direction = _direction.RotateClockwise(rotSteps);

        if (!_hasListened) {
            _hasListened = true;
            TurnSystem.OnInstanceCreated += x => {
                x.OnStepStart.AddListener(() => {
                    _progress = new();
                });
            };
        }
    }

    public override void OnEnter(MapObject dynamic) {
        Scheduler.AddRoutine(ConveyorRoutine(dynamic));
    }

    IEnumerator ConveyorRoutine(MapObject dynamic) {
        if (!_progress.ContainsKey(dynamic)) {
            _progress[dynamic] = startProgress;
        } else if (_progress[dynamic] - _cost < 0) {
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