using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int _steps;
    [SerializeField] Vector2Int _direction = new(1, 0);
    [SerializeField] bool _relative = true;

    public override bool CanPlace(Player player, int register) => true;

    public override IEnumerator ExecuteRoutine(Player player, int register) {
        for (var i = 0; i < _steps; i++) {
            yield return player.Model.Move(_direction, _relative);
            yield return TaskScheduler.DefaultTaskDelay;
        }
    }
}