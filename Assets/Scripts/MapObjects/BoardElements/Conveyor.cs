using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Serialization;

public class Conveyor : BoardElement<Conveyor, IMapObject>, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;
    [SerializeField] ConveyorRotation[] _rotation;
    
    const float StartProgress = 1f;
    const float MoveSpeed = 3f;
    const LeanTweenType EaseType = LeanTweenType.linear;
    
    static readonly Dictionary<IMapObject, float> _progress = new();
    static readonly List<(Vector2Int pos, bool final, MapEvent mapEvent)> _moves = new();
    
    public string Header => "Conveyor";
    public string Description => $"Moves objects {StringUtils.FormatMultiple(1f / _cost, "tile")} after each register.";

    protected override void Awake() {
        base.Awake();
        _direction = Rotator.Rotate(_direction);
        _rotation = _rotation.Select(r => {
            var rot = r._rotation;
            if (Rotator.FlipX) rot *= -1;
            if (Rotator.FlipY) rot *= -1;
            
            return new ConveyorRotation {
                _rotation = rot,
                _relativeDirection = Rotator.Rotate(r._relativeDirection)
            };
        }).ToArray();

        OnRotationChanged += _ => {
            Debug.LogWarning("Conveyor rotation is not supported!", this);
        };
    }
    
    public static new bool ActivateElement() {
        _progress.Clear();
        _moves.Clear();
        
        if (ActiveElements == 0) return false;
        
        OnActivateEvent?.Invoke();

        // Execute moves in reverse order they were added
        if (_moves.Count == 0) return false;
        for (var i = _moves.Count - 1; i >= 0; i--){
            var (_, _, mapEvent) = _moves[i];
            
            var routine = Interaction.EaseEvent(mapEvent, EaseType, MoveSpeed);
            TaskScheduler.PushRoutine(routine, delay: 0);
        }
        return true;
    }

    protected override void Activate(IMapObject[] targets) {
        if (targets.Length == 0) return;
        
        // Get objects to move with enough progress
        var movable = targets
            .Where(t => _progress.EnforceKey(t.Object, StartProgress) >= _cost)
            .ToList();
        
        if (movable.Count == 0) return;
        foreach (var obj in movable) {
            _progress[obj] -= _cost;
        }

        var targetPos = GridPos + _direction;
        
        // If another non-pushable object is ending on the same tile, neither object moves
        if (CheckForObstruction()) return;
        
        var targetTileFilled = MapSystem.TryGetTile(targetPos, out var targetTile);
        
        // Check if there is a conveyor at the target position
        if (!targetTileFilled || targetTile.FirstOrDefault(t => t is Conveyor) == null) {
            // If there's no conveyor, this is a final move
            // We are also assuming adjacent conveyors are accessible from each other
            
            if (Interaction.SoftMove(movable[0].Object, _direction, out var mapEvent)) {
                // Add remaining objects to the map event
                mapEvent.MapObjects.AddRange(movable.Skip(1).Select(o => o.Object));
                _moves.Add((targetPos, true, mapEvent));
            }
        } else {
            // If there is a conveyor, recursively activate it
            var next = (Conveyor)targetTile.First(t => t is Conveyor);
            var rot = next.GetRotation(-_direction);
                
            _moves.Add((targetPos, false, new MapEvent(movable, _direction, rot)));
            next.Activate(movable.ToArray());
        }

        bool CheckForObstruction() {
            // Check for any final moves that is obstructing this conveyor move
            var obstructingMoves = _moves
                .Where(m => m.final
                            && m.pos == targetPos
                            && m.mapEvent.MapObjects.Any(o => o is ICanEnterHandler))
                .Select(m => m.mapEvent)
                .ToArray();

            if (obstructingMoves.Length <= 0) return false;
            // Get obstacles on this conveyor
            var movableObstacles = movable
                .Select(o => o.Object)
                .OfType<ICanEnterHandler>()
                .ToArray();

            if (movableObstacles.Length <= 0) return true;

            // Remove all obstacles from the obstructing moves
            foreach (var move in obstructingMoves) {
                foreach (var obj in move.MapObjects.OfType<ICanEnterHandler>()) {
                    move.MapObjects.Remove(obj.Object);
                }
            }
            
            // Remove obstacles on this move
            foreach (var obstacle in movableObstacles) {
                movable.Remove(obstacle); 
            }

            return true;
        }
    }

    int GetRotation(Vector2Int dir){
        return _rotation.FirstOrDefault(r => r._relativeDirection == dir)._rotation;
    }

    [Serializable]
    struct ConveyorRotation {
        [FormerlySerializedAs("RelativeDirection")] 
        public Vector2Int _relativeDirection;
        [FormerlySerializedAs("Rotation")] 
        public int _rotation;
    }
}