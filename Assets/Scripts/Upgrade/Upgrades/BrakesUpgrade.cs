using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Brakes")]
public class BrakesUpgrade : UpgradeCardData {
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] ProgramCardData _replacementCard;
    [SerializeField] OverlayData<Choice<bool>> _overlay;

    readonly List<Player> _activePlayers = new();

    public override void OnAdd(Player player) {
        ExecutionPhase.PlayerRegister += OnRegister;
        _activePlayers.Add(player);
    }
    
    public override void OnRemove(Player player) {
        ExecutionPhase.PlayerRegister -= OnRegister;
        _activePlayers.Remove(player);
    }

    void OnRegister(ProgramExecution execution) {
        if (!_activePlayers.Contains(execution.Player)) return;
        if (execution.CurrentCard != _targetCard) return;
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
            execution.CardOverride = _replacementCard;
            Log.Message($"{Log.PlayerString(execution.Player)} treated their {Log.ProgramString(_targetCard)} as a {Log.ProgramString(_replacementCard)} with {Log.UpgradeString(this)}");
        }
    }
}