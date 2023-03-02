using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnergizeProgram", menuName = "ScriptableObjects/Programs/Energize")]
public class EnergizeProgram : ProgramCardData {
    [SerializeField] int _energy;

    public override bool CanPlace(Player player, int register) => true;

    public override IEnumerator ExecuteRoutine(Player player, int register)  {
        player.Energy.Value += _energy;
        yield break;
    }
}