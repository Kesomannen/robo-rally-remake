using System;

[Serializable]
public struct Damage {
    public int NumberOfCards;
    public ProgramCardData Card;
    public Pile Destination;
    public CardPlacement Placement;

    public void Apply(Player player) {
        var pile = player.GetCollection(Destination);
        for (var i = 0; i < NumberOfCards; i++) {
            pile.AddCard(Card, Placement);
        }
    }
}