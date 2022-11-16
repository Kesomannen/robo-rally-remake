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
    static readonly Dictionary<Vector2Int, MapHelper.MapAction> _moves = new();

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
        foreach (var (pos, move) in _moves) {
            Scheduler.Enqueue(MapHelper.EaseAction(move), $"Conveyor moving to {pos}");
        }

        yield return Scheduler.WaitUntilClearRoutine();
    }

    protected override void Activate(IMapObject[] targets) {

    }

    [Serializable]
    struct ConveyorRotation {
        public Vector2Int RelativeDirection;
        public int Rotation;
    }
}