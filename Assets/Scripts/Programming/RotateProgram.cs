using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateProgram", menuName = "ScriptableObjects/Programs/Rotate")]
public class RotateProgram : ProgramCardData {
    [SerializeField] int rotation;

    public override bool CanPlace(GamePlayer player, int positionInRegister) => true;

    public override IEnumerator Execute(GamePlayer player, int positionInRegister)  {
        yield return player.Model.Rotate(rotation);
    }
}