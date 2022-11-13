using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int steps;
    [SerializeField] Vector2Int direction;
    [SerializeField] bool relative;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        var moveVector = relative ? player.Model.RotateAsObject(direction) : direction;
        for (int i = 0; i < steps; i++) {
            Scheduler.Push(Interaction.PushRoutine(player.Model, moveVector), $"MoveProgram {player} in {moveVector}");
        }
        yield break;
    }
}