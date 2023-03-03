using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ModularChassisAffector", menuName = "ScriptableObjects/Affectors/Modular Chassis")]
public class ModularChassisAffector : ScriptableAffector<IPlayer> {
    [SerializeField] UpgradeCardData _modularChassis;
    [SerializeField] OverlayData<Choice<UpgradeCardData>> _takeUpgradeOverlay;

    public override void Apply(IPlayer target) {
        target.Owner.Model.OnPush += OnPush;
    }

    public override void Remove(IPlayer target) {
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
                    Message = $"taking an upgrade from {receiver}",
                    OutputArray = result,
                    AvailablePredicate = c => !pusher.Upgrades.Contains(c),
                    MinChoices = 1
                });
                takenUpgrade = result[0];
            }

            pusher.RemoveUpgrade(_modularChassis);
            receiver.RemoveUpgrade(takenUpgrade);
            yield return pusher.GetSlotAndAdd(takenUpgrade);
            yield return receiver.GetSlotAndAdd(_modularChassis);
        }
    }
}