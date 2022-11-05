using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player {
    public ProgramCardData[] Registers { get; private set; } = new ProgramCardData[5];

    public readonly PlayerModel Model;
    public readonly RobotData RobotData;

    public readonly ulong ClientId;
    public readonly ClampedField<int> Energy;
    public readonly ObservableField<Checkpoint> CurrentCheckpoint;

    public readonly CardCollection Hand, DrawPile, DiscardPile;

    const float _drawDelay = 0.5f;

    public event Action OnShuffleDeck;
    public event Action<ProgramCardData> OnDraw, OnDiscard;

    public Player(PlayerArgs args) {
        ClientId = args.OwnerId;
        RobotData = args.RobotData;

        Hand = new(maxCards: args.HandSize);
        DrawPile = new(startingCards: args.RobotData.StartingDeck);
        DrawPile.Shuffle();
        DiscardPile = new();

        CurrentCheckpoint = new (
            initialValue: args.SpawnPoint
        );

        Energy = new (
            initialValue: args.StartingEnergy,
            min: 0,
            max: args.MaxEnergy
        );

        Model = MapSystem.Instance.CreateObject (
            args.ModelPrefab,
            args.SpawnPoint.GridPos
        );

        Model.Init(this);
        Model.GetComponent<SpriteRenderer>().sprite = args.RobotData.Sprite;
    }

    public int GetBonusPriority () {
        return 0;
    }

    public CardCollection GetCollection(Pile target) {
        return target switch {
            Pile.Hand => Hand,
            Pile.DrawPile => DrawPile,
            Pile.DiscardPile => DiscardPile,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    public void ShuffleDeck() {
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
        card.OnDraw(this);

        OnDraw?.Invoke(card);
    }

    public void DrawCards(int count) {
        for (int i = 0; i < count; i++) {
            DrawCard();
        };
    }

    public void DrawCardsUpTo(int count) {
        DrawCards(Mathf.Max(0, count - Hand.Cards.Count));
    }

    public void DiscardCard(int index) {
        if (Hand.Cards.Count == 0) return;

        var card = Hand[index];
        Hand.RemoveCard(index);

        DiscardPile.AddCard(card, CardPlacement.Top);
        card.OnDiscard(this);

        OnDiscard?.Invoke(card);
    }

    public void DiscardCard(ProgramCardData card) {
        DiscardCard(Hand.Cards.IndexOf(card));
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

public struct PlayerArgs {
    public Checkpoint SpawnPoint;
    public ulong OwnerId;
    public RobotData RobotData;
    public PlayerModel ModelPrefab;
    public int MaxEnergy;
    public int StartingEnergy;
    public int HandSize;
}