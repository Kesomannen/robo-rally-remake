using System;
using System.Collections.Generic;

[Serializable]
public class Damage {
    public List<ProgramCardData> Cards;
    public Pile Destination;
    public CardPlacement Placement;

    public void Apply(Player player) {
        var pile = player.GetCollection(Destination);
        foreach (var card in Cards){
            pile.AddCard(card, Placement);
        }
    }
}