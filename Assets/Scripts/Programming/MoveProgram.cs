using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int _steps;
    [SerializeField] Vector2Int _direction = new(1, 0);
    [SerializeField] bool _relative = true;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        var moveVector = _relative ? player.Model.Rotator.Rotate(_direction) : _direction;
        for (var i = 0; i < _steps; i++) {
            Scheduler.Push(Move(), $"Move Program for {player} in direction {moveVector}");
        }
        yield break;

        IEnumerator Move() {
            if (Interaction.Push(player.Model, moveVector, out var action)) {
                yield return Interaction.EaseEvent(action);
            }
        }
    }
}