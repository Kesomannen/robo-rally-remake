using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Mini Howitzer")]
public class MiniHowitzer : UpgradeCardData {
    [SerializeField] CardAffector _cardAffector;
    [SerializeField] bool _countAsPush;
    [SerializeField] OverlayData<Choice<bool>> _overlay;

    public override void OnAdd(Player player) {
        player.Model.OnShoot += OnShoot;
    }

    public override void OnRemove(Player player) {
        player.Model.OnShoot -= OnShoot;
    }

    void OnShoot(PlayerModel.CallbackContext context) {
        var attacker = context.Attacker;
        if (attacker.Energy.Value < 1) return;
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new bool[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<bool> {
                Overlay = _overlay,
                Player = attacker,
                Options = new[] { true, false },
                Message = "considering Mini Howitzer",
                OutputArray = result,
                MinChoices = 1
            });
            if (!result[0]) yield break;
            Log.Instance.RawMessage($"{Log.PlayerString(attacker)} spent {Log.EnergyString(1)} to push {Log.PlayerString(context.Target)} with {Log.UpgradeString(this)}");
            if (Interaction.Push(context.Target.Model, context.OutgoingDirection, out var mapEvent)) {
                yield return Interaction.EaseEvent(mapEvent);
                if (_countAsPush) attacker.Model.RegisterPush(mapEvent);
            }
            
            context.Target.ApplyCardAffector(_cardAffector);
            attacker.Energy.Value--;
        }
    }
}