using System;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    public readonly PlayerModel Model;
    public readonly ObservableField<int> Energy;

    ProgramCardData[] _registers;
    public IReadOnlyList<ProgramCardData> Registers => _registers;

    public readonly CardCollection Hand, DrawPile, DiscardPile;

    public Player(Vector2Int gridPos) {
        Model = MapSystem.instance.CreateMapObject(
            PlayerManager.instance.PlayerModelPrefab,
            gridPos
        ) as PlayerModel;

        Hand = new(maxCards: PlayerManager.instance.HandSize);
        DrawPile = new(cards: PlayerManager.instance.StartingDeck);
        DiscardPile = new();

        Energy = new(PlayerManager.instance.StartingEnergy);
    }

    public CardCollection GetCollection(Pile type) {
        return type switch {
            Pile.Hand => Hand,
            Pile.DrawPile => DrawPile,
            Pile.DiscardPile => DiscardPile,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public enum Pile {
    Hand,
    DrawPile,
    DiscardPile,
}