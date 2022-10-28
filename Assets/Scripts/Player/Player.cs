using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player {
    public readonly PlayerModel Model;
    public readonly ClampedField<int> Energy;

    public ProgramCardData[] Registers { get; private set; } = new ProgramCardData[5];

    public readonly CardCollection Hand, DrawPile, DiscardPile;

    public event Action OnShuffleDeck;
    public event Action<ProgramCardData> OnDraw, OnDiscard;

    public Player(Vector2Int spawnPos) {
        Model = MapSystem.Instance.CreateMapObject(
            PlayerManager.Instance.PlayerModelPrefab,
            spawnPos
        ) as PlayerModel;

        Hand = new(maxCards: PlayerManager.Instance.HandSize);
        DrawPile = new(startingCards: PlayerManager.Instance.StartingDeck);
        DiscardPile = new();

        Energy = new (
            PlayerManager.Instance.StartingEnergy,
            PlayerManager.Instance.MaxEnergy,
            0
        );
    }

    public int GetBonusPriority () {
        return 0;
    }

    public CardCollection GetCollection(Pile type) {
        return type switch {
            Pile.Hand => Hand,
            Pile.DrawPile => DrawPile,
            Pile.DiscardPile => DiscardPile,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    void ShuffleDeck() {
        DrawPile.AddRange(DiscardPile.Cards, CardPlacement.Top);
        DiscardPile.Clear();
        DrawPile.Shuffle();
        OnShuffleDeck?.Invoke();
    }

    public void DrawCard() {
        if (DrawPile.Cards.Count == 0) ShuffleDeck();

        var card = DrawPile[0];
        DrawPile.RemoveCard(0);
        Hand.AddCard(card, CardPlacement.Top);
        OnDraw?.Invoke(card);
    }

    public void DrawCards(int count) {
        for (int i = 0; i < count; i++) DrawCard();
    }

    public void DrawCardsUpTo(int count) {
        DrawCards(Mathf.Max(0, count - Hand.Cards.Count));
    }

    public void DiscardCard(int index) {
        var card = Hand[index];
        Hand.RemoveCard(index);
        DiscardPile.AddCard(card, CardPlacement.Top);
        OnDiscard?.Invoke(card);
    }

    public void DiscardCard(ProgramCardData card) {
        DiscardCard(Hand.Cards.IndexOf(card));
    }

    public void DiscardCardsDownTo(int count) {
        while (Hand.Cards.Count > count) DiscardCard(Hand.Cards.Count - 1);
    }

    public void DiscardCards(IReadOnlyList<ProgramCardData> cards) {
        for (int i = 0; i < cards.Count; i++) DiscardCard(cards[i]);
    }

    public void DiscardRandomCard() {
        DiscardCard(Random.Range(0, Hand.Cards.Count));
    }

    public void DiscardRandomCards(int count) {
        for (int i = 0; i < count; i++) DiscardRandomCard();
    }
}

public enum Pile {
    Hand,
    DrawPile,
    DiscardPile,
}