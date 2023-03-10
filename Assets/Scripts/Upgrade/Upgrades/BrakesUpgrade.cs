using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Brakes")]
public class BrakesUpgrade : UpgradeCardData {
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] ProgramCardData _replacementCard;
    [SerializeField] OverlayData<Choice<bool>> _overlay;
    
    public override void OnAdd(Player player) {
        player.OnProgramCardExecuted += OnExecute;
    }
    
    public override void OnRemove(Player player) {
        player.OnProgramCardExecuted -= OnExecute;
    }

    void OnExecute(ProgramExecution execution) {
        if (execution.Card != _targetCard) return;
        execution.Card = _replacementCard;
    }
}