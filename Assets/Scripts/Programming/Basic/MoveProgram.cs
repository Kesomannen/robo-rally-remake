using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveProgram", menuName = "ScriptableObjects/Programs/Move")]
public class MoveProgram : ProgramCardData {
    [SerializeField] int _steps;
    [SerializeField] Vector2Int _direction = new(1, 0);
    [SerializeField] bool _relative = true;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        for (var i = 0; i < _steps; i++){
            TaskScheduler.PushRoutine(player.Model.Move(_direction, _relative));
        }
        yield break;
    }
}