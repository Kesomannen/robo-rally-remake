using UnityEngine;

[CreateAssetMenu(fileName = "RotateProgram", menuName = "ScriptableObjects/Programs/Rotate")]
public class RotateProgram : ProgramCardData {
    [SerializeField] int rotation;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IScheduleItem Execute(Player player, int positionInRegister)  {
        return new ScheduleRoutine(player.Model.Rotate(rotation));
    }
}