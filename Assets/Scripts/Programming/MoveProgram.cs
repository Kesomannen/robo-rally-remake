using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/MoveProgram")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int steps;
    [SerializeField] Vector2Int direction;
    [SerializeField] bool relative;

    public override void Execute(Player player, int positionInRegister)  {
        var moveVector = relative ? direction.RotateAsTransform(player.Model.transform) : direction;
        for (int i = 0; i < steps; i++) {
            InteractionSystem.Push(player.Model, moveVector, out var scheduleItem);
            Scheduler.AddItem(scheduleItem);
        }
    }
}