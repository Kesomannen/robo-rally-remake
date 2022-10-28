using UnityEngine;

[CreateAssetMenu(fileName = "RepeatProgram", menuName = "ScriptableObjects/Programs/Program")]
public class RepeatProgram : ProgramCardData {
    [SerializeField] int repeatCount;
    [SerializeField] int relativeRepeatIndex;

    public override bool CanPlace(Player player, int positionInRegister) {
        var posToRepeat = positionInRegister + relativeRepeatIndex;
        return posToRepeat > 0 && posToRepeat < ExecutePhase.RegisterCount;
    }

    public override IScheduleItem Execute(Player player, int positionInRegister)  {
        var posToRepeat = positionInRegister + relativeRepeatIndex;
        var group = new ScheduleGroup();
        for (int i = 0; i < repeatCount; i++) {
            var card = player.Registers[posToRepeat];
            group.AddItem(card.Execute(player, posToRepeat));
        }
        return group;
    }
}