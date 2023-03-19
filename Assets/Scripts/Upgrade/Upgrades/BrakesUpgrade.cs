using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Brakes")]
public class BrakesUpgrade : UpgradeCardData {
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] ProgramCardData _replacementCard;
    [SerializeField] OverlayData<Choice<bool>> _overlay;
    
    public override void OnAdd(Player player) {
        ExecutionPhase.OnPlayerRegister += OnRegister;
    }
    
    public override void OnRemove(Player player) {
        ExecutionPhase.OnPlayerRegister -= OnRegister;
    }

    void OnRegister(ProgramExecution execution) {
        if (execution.Card != _targetCard) return;
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new bool[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<bool> {
                Overlay = _overlay,
                Player = execution.Player,
                Options = new[] { true, false },
                Message = "considering Brakes",
                OutputArray = result,
                MinChoices = 1
            });
            if (!result[0]) yield break;
            execution.Card = _replacementCard;
            Log.Instance.RawMessage($"{Log.PlayerString(execution.Player)} treated their {Log.ProgramString(_targetCard)} as a {Log.ProgramString(_replacementCard)} with {Log.UpgradeString(this)}");
        }
    }
}