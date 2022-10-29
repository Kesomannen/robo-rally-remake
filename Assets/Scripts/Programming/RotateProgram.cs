using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateProgram", menuName = "ScriptableObjects/Programs/Rotate")]
public class RotateProgram : ProgramCardData {
    [SerializeField] int rotation;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator Execute(Player player, int positionInRegister)  {
        yield return player.Model.Rotate(rotation);
    }
}