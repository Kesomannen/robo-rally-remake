using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateProgram", menuName = "ScriptableObjects/Programs/Rotate")]
public class RotateProgram : ProgramCardData {
    [SerializeField] int _rotation;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        yield return player.Model.RotateRoutine(_rotation);
    }
}