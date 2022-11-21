using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

public class Conveyor : BoardElement<Conveyor, IMapObject>, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;
    [SerializeField] ConveyorRotation[] _rotation;
    
    const float StartProgress = 1f;
    const float MoveSpeed = 3f;
    const LeanTweenType EaseType = LeanTweenType.linear;
    
    static readonly Dictionary<IMapObject, float> _progress = new();
    static readonly Dictionary<Vector2Int, MapEvent> _moves = new();
    
    public string Header => "Conveyor";
    public string Description => $"Moves objects {1f / _cost} tile after each register.";

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
        foreach (var (pos, move) in _moves){
            var routine = Interaction.EaseEvent(move, EaseType, MoveSpeed);
            Scheduler.Enqueue(routine, $"Conveyor moving to {pos}", 0);
        }

        yield return Scheduler.WaitUntilClearRoutine();
    }

    protected override void Activate(IMapObject[] targets){
        var movable = targets
            .Where(t => _progress.EnforceKey(t.Object, StartProgress) >= _cost)
            .ToList();
        
        if (movable.Count == 0) return;
        foreach (var obj in movable){
            _progress[obj] -= _cost;
        }

        var targetPos = GridPos + _direction;
        
        // If another non-pushable object is moving to the same spot, neither object moves
        var obstructingMoves = _moves
            .Where(m => m.Key == targetPos
                        && m.Value.MapObjects.Any(o => o is ICanEnterHandler))
            .Select(m => m.Value)
            .ToArray();

        if (obstructingMoves.Length > 0){
            var movableObstacles = movable
                .Select(o => o.Object)
                .OfType<ICanEnterHandler>()
                .ToArray();

            if (movableObstacles.Length > 0){
                foreach (var move in obstructingMoves){
                    foreach (var obj in move.MapObjects.OfType<ICanEnterHandler>()){
                        move.MapObjects.Remove(obj.Object);
                    }
                }
                foreach (var obstacle in movableObstacles){
                    movable.Remove(obstacle);
                }
            }
        }

        // Check if there is a conveyor at the target position
        var emptyTarget = !MapSystem.TryGetTile(targetPos, out var targetTile);
        if (emptyTarget){
            _moves.Add(targetPos, new MapEvent(movable, _direction));
        } else {
            var obj = targetTile.FirstOrDefault(t => t is Conveyor);
            if (obj == null){
                if (Interaction.SoftMove(movable[0].Object, _direction, out var mapEvent)){
                    // Add remaining objects to the map event
                    mapEvent.MapObjects.AddRange(movable.Skip(1).Select(o => o.Object));
                }
            } else {
                var next = (Conveyor)obj;
                var rot = next.GetRotation(-_direction);
                _moves.Add(targetPos, new MapEvent(movable, _direction, rot));
                next.Activate(movable.ToArray());
            }
        }
    }

    int GetRotation(Vector2Int dir){
        return _rotation.FirstOrDefault(r => r.RelativeDirection == dir).Rotation;
    }

    [Serializable]
    struct ConveyorRotation {
        public Vector2Int RelativeDirection;
        public int Rotation;
    }
}