using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CardAffector : IPermanentAffector<IPlayer> {
    [SerializeField] List<ProgramCardData> _cards;
    [SerializeField] Pile _destination;
    [SerializeField] CardPlacement _placement;
    
    public List<ProgramCardData> Cards => _cards;

    public Pile Destination {
        get => _destination;
        set => _destination = value;
    }
    
    public CardPlacement Placement {
        get => _placement;
        set => _placement = value;
    }
    
    public CardAffector(IEnumerable<ProgramCardData> cards, Pile destination, CardPlacement placement) {
        _cards = cards.ToList();
        _destination = destination;
        _placement = placement;
    }

    public void Apply(IPlayer player) => Apply(player.Owner, _cards, _destination, _placement);

    public static void Apply(Player player, IEnumerable<ProgramCardData> cards, Pile destination, CardPlacement placement) {
        var pile = player.GetCollection(destination);
        foreach (var card in cards){
            pile.AddCard(card, placement);
        }
    }
}