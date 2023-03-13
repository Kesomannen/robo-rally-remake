﻿using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Memory Swap")]
public class MemorySwapUpgrade : UpgradeCardData {
    [SerializeField] int _cardCount;
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        player.DrawCards(_cardCount);
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new ProgramCardData[_cardCount];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = player.Hand.Cards,
                Message = "choosing cards to discard with Memory Swap",
                OutputArray = result,
                MinChoices = 3
            });
            foreach (var card in result) {
                player.DiscardCard(card);
            }
        }
    }
}