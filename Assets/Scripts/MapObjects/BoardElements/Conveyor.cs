using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class Conveyor : BoardElement<Conveyor> {
    [SerializeField] Vector2Int _direction;
    [SerializeField] float _cost;

    const float _startProgress = 1f;

    static readonly Dictionary<DynamicObject, float> _progress = new();
    static readonly Dictionary<Vector2Int, DynamicObject> _moves = new();

    protected override void Awake() {
        base.Awake();
        _direction = RotateAsObject(_direction);
    }

    public static new IEnumerator ActivateRoutine() {
        _progress.Clear();
        _moves.Clear();

        _onActivate?.Invoke();

        // Execute moves
        foreach (var move in _moves) {
            var dynamic = move.Value;
            var dir = move.Value.GridPos - move.Key;

            if (Interaction.SoftMove(dynamic, dir, out var routine)) {
                Scheduler.Enqueue(routine);
            }
        }

        yield return Scheduler.WaitUntilClearRoutine();
    }

    protected override void Activate(DynamicObject dynamic) {
        var currentProgress = _progress.EnforceKey(dynamic, _startProgress);
        if (currentProgress < _cost) return;

        var nextPos = GridPos + _direction;

        if (_moves.ContainsKey(nextPos)) {
            // In this case, nothing should move
            var other = _moves[nextPos];
            foreach (var move in _moves.Where(x => x.Value == other).ToList()) {
                _moves.Remove(move.Key);
            }
        } else {
            _progress[dynamic] = currentProgress - _cost;
            _moves.Add(nextPos, dynamic);

            // Recursively call next conveyor in chain
            var isEmpty = MapSystem.Instance.TryGetTile(nextPos, out var tile);
            if (isEmpty) return;

            foreach (var obj in tile) {
                if (tile is Conveyor conveyor) {
                    conveyor.Activate(dynamic);
                    break;
                }
            }
        }
    }
}