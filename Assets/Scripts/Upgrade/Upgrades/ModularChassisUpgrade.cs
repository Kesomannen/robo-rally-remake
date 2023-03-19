using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Modular Chassis")]
public class ModularChassisUpgrade : UpgradeCardData {
    [SerializeField] OverlayData<Choice<UpgradeCardData>> _takeUpgradeOverlay;
    
    public override void OnAdd(Player target) {
        target.Model.OnPush += OnPush;
    }

    public override void OnRemove(Player target) {
        target.Owner.Model.OnPush -= OnPush;
    }
    
    void OnPush(PlayerModel.CallbackContext context) {
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            var pusher = context.Attacker;
            var receiver = context.Target;
            var receiverUpgrades = receiver.Upgrades.Where(c => c != null).ToArray();

            if (receiverUpgrades.Length == 0) yield break;

            UpgradeCardData takenUpgrade;

            if (receiverUpgrades.Length == 1) {
                takenUpgrade = receiverUpgrades[0];
            } else {
                var result = new UpgradeCardData[1];
                yield return ChoiceSystem.DoChoice(new ChoiceData<UpgradeCardData> {
                    Overlay = _takeUpgradeOverlay,
                    Player = pusher,
                    Options = receiverUpgrades,
                    Message = $"taking an upgrade from {receiver} with Modular Chassis",
                    OutputArray = result,
                    AvailablePredicate = c => !pusher.Upgrades.Contains(c),
                    MinChoices = 1
                });
                takenUpgrade = result[0];
            }

            Log.Instance.RawMessage($"{Log.PlayerString(pusher)} took {Log.UpgradeString(takenUpgrade)} from {Log.PlayerString(receiver)} with {Log.UpgradeString(this)}");
            
            pusher.RemoveUpgrade(this);
            receiver.RemoveUpgrade(takenUpgrade);
            yield return pusher.GetSlotAndAdd(takenUpgrade);
            yield return receiver.GetSlotAndAdd(this);
        }
    }
}