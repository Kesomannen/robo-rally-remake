using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RebootProgram", menuName = "ScriptableObjects/Programs/Reboot")]
public class RebootProgram : ProgramCardData {
    public override bool CanPlace(Player player, int positionInRegister) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        player.Reboot();
        yield break;
    }
}