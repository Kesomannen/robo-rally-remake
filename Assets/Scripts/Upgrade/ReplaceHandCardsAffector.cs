using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "ReplaceHandCardsAffector", menuName = "ScriptableObjects/Affectors/Replace Hand Cards")]
public class ReplaceHandCardsAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] FilterType _filterType;
    [ShowIf("_filterType", ShowIfComparer.Equals)]
    [SerializeField] ProgramCardData.CardType _cardType;
    [ShowIf("_filterType", ShowIfComparer.Equals, (int)FilterType.Specific)]
    [SerializeField] ProgramCardData _card;
    [SerializeField] bool _drawReplacements;
    [SerializeField] bool _discardPermanently;
    [Space]
    [SerializeField] bool _random;
    [SerializeField] int _amount;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] int _minCards;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;
    [ShowIf("_random", ShowIfComparer.False)]
    [SerializeField] float _time = 30f;
    
    public override void Apply(IPlayer player) {
        var plr = player.Owner;
        var matchingCards = (_filterType switch {
            FilterType.Type => plr.Hand.Cards.Where(c => c.Type == _cardType),
            FilterType.Specific => plr.Hand.Cards.Where(c => c == _card),
            FilterType.All => plr.Hand.Cards,
            _ => throw new ArgumentOutOfRangeException()
        }).ToList();
        
        var cardsToDiscard = Mathf.Min(matchingCards.Count, _amount);
        if (cardsToDiscard <= 0) return;
        
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            if (_random) {
                var cardsToKeep = matchingCards.Count - cardsToDiscard;
                for (var i = 0; i < cardsToKeep; i++) {
                    matchingCards.RemoveAt(Random.Range(0, matchingCards.Count));
                }
            } else {
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
                matchingCards = result.ToList();
            }
            
            foreach (var card in matchingCards) {
                plr.DiscardCard(card);
                plr.DiscardPile.RemoveCard(card);
            }

            if (_drawReplacements) {
                plr.DrawCards(cardsToDiscard);
            }
        }
    }

    enum FilterType {
        Type,
        Specific,
        All
    }
}