using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int steps;
    [SerializeField] Vector2Int direction;
    [SerializeField] bool relative;

    public override bool CanPlace(GamePlayer player, int positionInRegister) => true;

    public override IEnumerator Execute(GamePlayer player, int positionInRegister)  {
        var moveVector = relative ? player.Model.RotateVector(direction) : direction;
        for (int i = 0; i < steps; i++) {
            if (InteractionSystem.Push(player.Model, moveVector, out var routineList)) {
                foreach (var routine in routineList) {
                    yield return Scheduler.RoutineGroup(routineList);
                }
            } else {
                break;
            }
        }
    }
}