using System.Collections;
using UnityEngine;

public class BoinkUpgrade : UpgradeCardData {
    [SerializeField] OverlayData<Choice<Vector2Int>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeSystem.BeforeRegister.Active || UpgradeSystem.AfterRegister.Active;
    }
    
    public override void OnAdd(Player player) {
        UpgradeSystem.BeforeRegister.AddListener();
        UpgradeSystem.AfterRegister.AddListener();
    }

    public override void OnRemove(Player player) {
        UpgradeSystem.BeforeRegister.RemoveListener();
        UpgradeSystem.AfterRegister.RemoveListener();
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
            yield return model.Move(result[0], false);   
        }
    }
}