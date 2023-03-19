using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Refresh")]
public class RefreshUpgrade : UpgradeCardData {
    [SerializeField] ProgramCardData[] _options;
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override void OnAdd(Player player) {
        UpgradeAwaiter.BeforeRegister.AddListener(player);
    }

    public override void OnRemove(Player player) {
        UpgradeAwaiter.BeforeRegister.RemoveListener(player);
    }

    public override bool CanUse(Player player) {
        return UpgradeAwaiter.BeforeRegister.ActiveFor(player);
    }

    public override void Use(Player player) {
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            var registerIndex = ExecutionPhase.CurrentRegister;
            var oldCard = player.Program[registerIndex];

            var result = new ProgramCardData[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = _options,
                Message = "choosing a card with Refresh",
                OutputArray = result,
                AvailablePredicate = programCard => programCard.CanPlace(player, registerIndex),
                MinChoices = 1
            });

            if (oldCard.Type != ProgramCardData.CardType.Damage) {
                player.DiscardPile.AddCard(oldCard, CardPlacement.Top);
            }
            player.Program.SetRegister(registerIndex, result[0]);

            Log.Instance.RawMessage($"{Log.PlayerString(player)} replaced {Log.ProgramString(oldCard)} with {Log.ProgramString(result[0])} in their register");
        }
    }
}