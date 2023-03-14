using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Move")]
public class MoveUpgrade : UpgradeCardData {
    [SerializeField] OverlayData<Choice<Vector2Int>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeAwaiter.BeforeRegister.ActiveFor(player)
               || UpgradeAwaiter.AfterRegister.ActiveFor(player);
    }
    
    public override void OnAdd(Player player) {
        UpgradeAwaiter.BeforeRegister.AddListener(player);
        UpgradeAwaiter.AfterRegister.AddListener(player);
    }

    public override void OnRemove(Player player) {
        UpgradeAwaiter.BeforeRegister.RemoveListener(player);
        UpgradeAwaiter.AfterRegister.RemoveListener(player);
    }

    public override void Use(Player player) {
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new Vector2Int[1];
            var model = player.Owner.Model;

            yield return ChoiceSystem.DoChoice(new ChoiceData<Vector2Int> {
                Overlay = _overlay,
                Player = player.Owner,
                Options = DirectionChoice.DirectionsWithoutZero,
                Message = "choosing a direction to move with Boink",
                OutputArray = result,
                MinChoices = 1
            });
            yield return model.MoveSteps(result[0], false, 1);   
        }
    }
}