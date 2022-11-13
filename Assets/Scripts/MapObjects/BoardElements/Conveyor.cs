using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

public class Conveyor : BoardElement<Conveyor, IMapObject> {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;
    [SerializeField] ConveyorRotation[] _rotation;

    const float _startProgress = 1f;

    static readonly Dictionary<MapObject, float> _progress = new();
    static readonly Dictionary<Vector2Int, (MapObject obj, int rot)> _moves = new();

    protected override void Awake() {
        base.Awake();
        _direction = Rotator.Rotate(_direction);
        _rotation = _rotation.Select(r => new ConveyorRotation() {
            Rotation = r.Rotation,
            RelativeDirection = Rotator.Rotate(r.RelativeDirection)
        }).ToArray();

        OnRotationChanged += s => {
            _direction = _direction.Transform(s);
            _rotation = _rotation.Select(r => new ConveyorRotation() {
                Rotation = r.Rotation,
                RelativeDirection = r.RelativeDirection.Transform(s)
            }).ToArray();
        };
    }
    
    public static new IEnumerator ActivateElement() {
        _progress.Clear();
        _moves.Clear();
        
        OnActivateEvent?.Invoke();

        // Execute moves
        foreach (var move in _moves) {
            var mapObject = move.Value.obj;
            var dir = move.Key - mapObject.GridPos;
            var rot = move.Value.rot;

            if (Interaction.SoftMove(mapObject, dir, out var moveRoutine)) {
                if (rot == 0) {
                    Scheduler.Enqueue(moveRoutine, $"Conveyor {mapObject} in {dir}");
                } else {
                    var rotationRoutine = mapObject.RotateRoutine(move.Value.rot);
                    var routineList = new IEnumerator[2] { moveRoutine, rotationRoutine };
                    var routine = Scheduler.GroupRoutines(routineList);

                    Scheduler.Enqueue(routine, $"Conveyor {mapObject} in {dir}");
                }
            }
        }

        yield return Scheduler.WaitUntilClearRoutine();
    }

    protected override void Activate(IMapObject[] targets) {
        foreach (var target in targets) {
            var mapObject = target.Object;

            var currentProgress = _progress.EnforceKey(mapObject, _startProgress);
            if (currentProgress < _cost) return;

            var nextPos = GridPos + _direction;

            if (_moves.ContainsKey(nextPos)) {
                // In this case, nothing should move
                var other = _moves[nextPos];
                foreach (var move in _moves.Where(x => x.Value == other).ToList()) {
                    _moves.Remove(move.Key);
                }
            } else {
                _progress[mapObject] = currentProgress - _cost;

                // Rotate only if object was moved by conveyor
                var rotation = 0;
                if (_rotation.Length != 0) {
                    var delta = mapObject.GridPos - GridPos;
                    if (delta != Vector2Int.zero) {
                        delta.x /= Mathf.Abs(delta.x);
                        delta.y /= Mathf.Abs(delta.y);

                        var rotationObj = _rotation.FirstOrDefault(r => r.RelativeDirection == delta).Rotation;
                    }
                }
                _moves.Add(nextPos, (mapObject, rotation));

                // Recursively call next conveyor
                var isFilled = MapSystem.Instance.TryGetTile(nextPos, out var tile);
                if (!isFilled) return;

                foreach (var obj in tile) {
                    Debug.Log($"Checking {obj} for conveyor", obj);
                    if (obj is Conveyor conveyor) {
                        conveyor.Activate(new IMapObject[1] { mapObject });
                        break;
                    }
                }
            } 
        }
    }

    [Serializable]
    struct ConveyorRotation {
        public Vector2Int RelativeDirection;
        public int Rotation;
    }
}