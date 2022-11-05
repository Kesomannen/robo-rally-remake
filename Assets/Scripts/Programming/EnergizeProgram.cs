using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EnergizeProgram", menuName = "ScriptableObjects/Programs/Energize")]
public class EnergizeProgram : ProgramCardData {
    [SerializeField] int energy;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        player.Energy.Value += energy;
        yield break;
    }
}