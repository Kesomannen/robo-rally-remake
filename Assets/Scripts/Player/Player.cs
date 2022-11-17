using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player {
    public readonly PlayerModel Model;
    public readonly RobotData RobotData;

    public readonly ulong ClientId;
    public readonly ClampedField<int> Energy;
    public readonly ObservableField<int> CurrentCheckpoint;

    public readonly CardCollection Hand, DrawPile, DiscardPile;
    public readonly Program Program;

    public Damage RebootDamage { get; set; }
    public Damage LaserDamage { get; set; }

    public bool IsRebooted { get; private set; }

    public event Action OnShuffleDeck;
    public event Action<ProgramCardData> OnDraw, OnDiscard;

    public Player(PlayerArgs args) {
        ClientId = args.OwnerId;
        RobotData = args.RobotData;

        Hand = new CardCollection(maxCards: args.HandSize);
        DrawPile = new CardCollection(startingCards: args.RobotData.StartingDeck);
        DiscardPile = new CardCollection();
        Program = new Program(args.RegisterCount);
        
        DrawPile.Shuffle();

        RebootDamage = args.RebootDamage;
        LaserDamage = RobotData.LaserDamage;

        CurrentCheckpoint = new ObservableField<int>(
            initialValue: 0
        );

        Energy = new ClampedField<int>(
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

        ExecutionPhase.OnPhaseEnd += () => {
            IsRebooted = false;
        };
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

    public ProgramCardData GetTopCard() {
        if (DrawPile.Cards.Count == 0) ShuffleDeck();
        var card = DrawPile.Cards[0];
        DrawPile.RemoveCard(0);
        return card;
    }

    public void DrawCard() {
        var card = GetTopCard();

        Hand.AddCard(card, CardPlacement.Top);
        card.OnDraw(this);

        OnDraw?.Invoke(card);
    }

    public void DrawCards(int count) {
        for (var i = 0; i < count; i++) {
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
        for (var i = 0; i < cards.Count; i++) DiscardCard(cards[i]);
    }

    public void DiscardHand() {
        var cards = Hand.Cards.Count;
        for (var i = 0; i < cards; i++) DiscardCard(0);
    }

    public void DiscardRandomCard() {
        DiscardCard(Random.Range(0, Hand.Cards.Count));
    }

    public void DiscardRandomCards(int count) {
        for (var i = 0; i < count; i++) DiscardRandomCard();
    }

    public void SerializeRegisters(out byte playerIndex, out byte[] registers) {
        playerIndex = (byte) PlayerManager.Players.IndexOf(this);
        registers = Program.Cards.Select(c => (byte) c.GetLookupId()).ToArray();
    }

    public void Reboot(IBoard board) {
        IsRebooted = true;

        board.Respawn(Model);
        RebootDamage.Apply(this);
        
        DiscardHand();
        DiscardProgram();
    }

    public void DiscardProgram() {
        for (var i = 0; i < Program.Cards.Count; i++) {
            var card = Program[i];
            if (card == null) continue;

            DiscardPile.AddCard(card, CardPlacement.Top);
            Program.SetCard(i, null);
        }
    }

    public override string ToString() => $"Player {ClientId}";
}

public enum Pile {
    Hand,
    DrawPile,
    DiscardPile,
}

public struct PlayerArgs {
    public RebootToken SpawnPoint;
    public ulong OwnerId;
    public RobotData RobotData;
    public PlayerModel ModelPrefab;
    public int MaxEnergy;
    public int StartingEnergy;
    public int HandSize;
    public int RegisterCount;
    public Damage RebootDamage;
}