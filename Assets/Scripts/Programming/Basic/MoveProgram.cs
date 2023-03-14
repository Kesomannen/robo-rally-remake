using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int _steps;
    [SerializeField] Vector2Int _direction = new(1, 0);
    [SerializeField] bool _relative = true;

    public override bool CanPlace(Player player, int register) => true;

    public override IEnumerator ExecuteRoutine(Player player, int register) {
        yield return player.Model.MoveSteps(_direction, _relative, _steps);
    }
}