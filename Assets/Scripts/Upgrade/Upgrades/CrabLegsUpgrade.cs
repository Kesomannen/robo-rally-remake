using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Crab Legs")]
public class CrabLegsUpgrade : UpgradeCardData {
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] OverlayData<Choice<Vector2Int>> _overlay;
    
    public override void OnAdd(Player player) {
        player.OnProgramCardExecuted += OnExecute;
    }
    
    public override void OnRemove(Player player) {
        player.OnProgramCardExecuted -= OnExecute;
    }

    void OnExecute(ProgramExecution execution) {
        if (execution.CurrentCard != _targetCard) return;
        execution.ExecutionEnd += _ => TaskScheduler.PushRoutine(OnAfterExecute(execution));
    }

    IEnumerator OnAfterExecute(ProgramExecution execution) {
        // We check again in case the card was changed by another upgrade
        if (execution.CurrentCard != _targetCard) yield break;
        if (execution.Player.IsRebooted.Value) yield break;
        var model = execution.Player.Model;
        
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.zero };
        for (var i = 0; i < dirs.Length; i++) {
            dirs[i] = model.Rotator.Rotate(dirs[i]);
        }
            
        var result = new Vector2Int[1];
        yield return ChoiceSystem.DoChoice(new ChoiceData<Vector2Int> {
            Overlay = _overlay,
            Player = execution.Player,
            Options = dirs,
            Message = "considering Crab Legs",
            OutputArray = result,
            MinChoices = 1
        });
            
        var dir = result[0];
        if (dir == Vector2Int.zero) yield break;

        Log.Instance.RawMessage($"{Log.PlayerString(execution.Player)} moved one space {Log.DirectionString(dir)} and forward with {Log.UpgradeString(this)}");
        TaskScheduler.PushRoutine(model.MoveSteps(new[] { dir, model.Rotator.Identity }, false));
    }
}