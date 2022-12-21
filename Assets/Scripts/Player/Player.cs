using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : IPlayer {
    # region Fields, Properties & Constructor
    // Networking
    readonly ulong _clientId;
    
    // Robot
    public readonly PlayerModel Model;
    public readonly RobotData RobotData;
    
    public readonly ObservableField<bool> IsRebooted;
    public readonly ObservableField<int> CurrentCheckpoint;

    // Cards
    public readonly CardCollection Hand, DrawPile, DiscardPile;
    public readonly Program Program;

    public int CardsPerTurn;
    
    public int BonusPriority;

    // Damage
    public readonly CardAffector RebootAffector;
    public readonly CardAffector LaserAffector;
    public readonly CardAffector PushAffector;

    // Upgrades
    readonly UpgradeCardData[] _upgrades;
    public IReadOnlyList<UpgradeCardData> Upgrades => _upgrades;
        
    public readonly ObservableField<int> Energy;

    // Implementations
    public Player Owner => this;
    public MapObject Object => Model;
    
    public override string ToString() => $"Player {_clientId}";
    
    // Events
    public event Action OnShuffleDeck;
    public event Action<ProgramCardData> OnDraw, OnDiscard;
    public event Action<UpgradeCardData, int> OnUpgradeAdded, OnUpgradeRemoved;
    public event Action<UpgradeCardData> OnUpgradeUsed;

    public Player(PlayerArgs args) {
        _clientId = args.OwnerId;
        RobotData = args.RobotData;

        // Cards
        Hand = new CardCollection(maxCards: args.HandSize);
        DrawPile = new CardCollection(startingCards: args.RobotData.StartingDeck);
        DiscardPile = new CardCollection();
        Program = new Program(args.RegisterCount);
        
        DrawPile.Shuffle();
        
        CardsPerTurn = args.CardsPerTurn;

        // Damage
        RebootAffector = args.RebootAffector;
        LaserAffector = RobotData.GetLaserDamage();
        PushAffector = RobotData.GetPushDamage();
        
        Energy = new ObservableField<int>(args.StartingEnergy);
        _upgrades = new UpgradeCardData[args.UpgradeSlots];
        
        // Robot
        CurrentCheckpoint = new ObservableField<int>(0);
        Model = MapSystem.Instance.CreateObject (
            args.ModelPrefab,
            args.SpawnPoint.GridPos
        );
        IsRebooted = new ObservableField<bool>(false);

        // Initialize
        Model.Init(this);
        RobotData.OnSpawn(this);

        ExecutionPhase.OnPhaseEnd += () => {
            IsRebooted.Value = false;
        };
    }
    
    #endregion

    # region Card Management
    public CardCollection GetCollection(Pile target) {
        return target switch {
            Pile.Hand => Hand,
            Pile.DrawPile => DrawPile,
            Pile.DiscardPile => DiscardPile,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    void ShuffleDeck() {
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

    public ProgramCardData DiscardTopCardsUntil(Func<ProgramCardData, bool> predicate){
        while (true){
            var card = GetTopCard();
            if (predicate(card)) return card;
            DiscardPile.AddCard(card, CardPlacement.Top);
        }
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
        }
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

    public void DiscardCard([NotNull] ProgramCardData card) {
        DiscardCard(Hand.Cards.IndexOf(card));
    }

    public void DiscardCards(IEnumerable<ProgramCardData> cards) {
        foreach (var t in cards) DiscardCard(t);
    }

    public void DiscardHand() {
        var cards = Hand.Cards.Count;
        for (var i = 0; i < cards; i++) DiscardCard(Hand.Cards.Count - 1);
    }

    public void DiscardRandomCard() {
        DiscardCard(Random.Range(0, Hand.Cards.Count));
    }

    public void DiscardRandomCards(int count) {
        for (var i = 0; i < count; i++) DiscardRandomCard();
    }
    
    public void DiscardProgram() {
        for (var i = 0; i < Program.Cards.Count; i++) {
            var card = Program[i];
            if (card == null) continue;

            DiscardPile.AddCard(card, CardPlacement.Top);
            Program.SetCard(i, null);
        }
    }
    #endregion

    #region Upgrades
    
    public int GetOpenUpgradeSlot() {
        for (var i = 0; i < _upgrades.Length; i++){
            if (_upgrades[i] == null) return i;
        }
        return -1;
    }

    public void AddUpgrade(UpgradeCardData upgrade, int index) {
        RemoveUpgrade(index);
        _upgrades[index] = upgrade;
        upgrade.OnBuy(this);
        OnUpgradeAdded?.Invoke(upgrade, index);
    }

    public void UseUpgrade(int index) {
        var upgrade = _upgrades[index];
        upgrade.Apply(this);
        OnUpgradeUsed?.Invoke(upgrade);

        if (upgrade.Type == UpgradeType.Temporary) {
            RemoveUpgrade(index);   
        }
    }
    
    public void RemoveUpgrade(int index) {
        var upgrade = _upgrades[index];
        if (upgrade == null) return;
        
        upgrade.Remove(this);
        _upgrades[index] = null;
        OnUpgradeRemoved?.Invoke(upgrade, index);
    }
    
    #endregion

    public void Reboot(IBoard board, bool takeDamage = true) {
        IsRebooted.Value = true;

        board.Respawn(Model);
        if (takeDamage) RebootAffector.Apply(this);
        
        DiscardHand();
        DiscardProgram();
    }
    
    public void Reboot(bool takeDamage = true) {
        Reboot(MapSystem.GetParentBoard(Model), takeDamage);
    }
    
    public void SerializeRegisters(out byte playerIndex, out byte[] registers) {
        playerIndex = (byte) PlayerManager.Players.IndexOf(this);
        registers = Program.Cards.Select(c => (byte) c.GetLookupId()).ToArray();
    }
}

public enum Pile {
    Hand = 2,
    DrawPile = 1,
    DiscardPile = 0,
}

public struct PlayerArgs {
    public RebootToken SpawnPoint;
    public ulong OwnerId;
    public RobotData RobotData;
    public PlayerModel ModelPrefab;
    public int StartingEnergy;
    public int CardsPerTurn;
    public int HandSize;
    public int RegisterCount;
    public CardAffector RebootAffector;
    public int UpgradeSlots;
}