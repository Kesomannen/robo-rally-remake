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
        var moveVector = relative ? player.Model.RotateVector(direction) : direction;
        for (int i = 0; i < steps; i++) {
            List<IEnumerator> routineList = new();
            if (Interaction.Push(player.Model, moveVector, routineList)) {
                yield return Scheduler.PlayListRoutine(routineList);
            } else {
                break;
            }
        }
    }
}