using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EnergizeProgram", menuName = "ScriptableObjects/Programs/Energize")]
public class EnergizeProgram : ProgramCardData {
    [SerializeField] int energy;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IScheduleItem Execute(Player player, int positionInRegister)  {
        return new ScheduleRoutine(Energize());

        IEnumerator Energize() {
            yield return null;
            player.Energy.Value += energy;
        }
    }
}