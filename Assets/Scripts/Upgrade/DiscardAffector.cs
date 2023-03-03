using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DiscardAffector", menuName = "ScriptableObjects/Affectors/Discard")]
public class DiscardAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] Type _type;
    [SerializeField] int _amount;
    [SerializeField] bool _random;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] int _minCards;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] float _time = 40f;
    
    public override void Apply(IPlayer player) {
        var plr = player.Owner;
        var cardsToDiscard = _type switch {
            Type.Constant => _amount,
            Type.DownTo => plr.Hand.Cards.Count - _amount,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (cardsToDiscard <= 0) return;

        if (_random) {
            plr.DiscardRandomCards(cardsToDiscard);
        } else {
            TaskScheduler.PushRoutine(Task());
        }
        
        IEnumerator Task() {
            var result = new ProgramCardData[cardsToDiscard];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = plr,
                Options = plr.Hand.Cards,
                Message = $"discarding {cardsToDiscard} cards",
                OutputArray = result,
                Time = _time,
                MinChoices = Mathf.Min(cardsToDiscard, _minCards)
            });
            foreach (var card in result) {
                plr.DiscardCard(card);
            }
        }
    }

    enum Type {
        Constant,
        DownTo
    }
}