using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateProgram", menuName = "ScriptableObjects/Programs/Rotate")]
public class RotateProgram : ProgramCardData {
    [SerializeField] int _rotation;

    public override bool CanPlace(Player player, int register) => true;

    public override IEnumerator ExecuteRoutine(Player player, int register){
        TaskScheduler.PushRoutine(player.Model.RotateRoutine(_rotation));
        yield break;
    }
}