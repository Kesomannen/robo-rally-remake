using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : IPlayer {
    # region Fields, Properties & Constructor
    readonly string _name;
    public readonly ulong ClientId;
    
    // Robot
    public PlayerModel Model { get; private set; }
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
    
    public Player Owner => this;
    public MapObject Object => Model;

    public override string ToString() => _name;
    
    public event Action<CardAffector> CardAffectorApplied;
    public event Action<ProgramCardData> OnDraw, OnDiscard;
    
    public event Action<UpgradeCardData, int> UpgradeAdded, UpgradeRemoved;
    public event Action<UpgradeCardData> UpgradeUsed;
    
    public event Action<ProgramExecution> ProgramCardExecuted; 

    public Player(PlayerArgs args) {
        _name = args.Name;
        ClientId = args.ClientId;
        RobotData = args.RobotData;

        var startingDeck = args.RobotData.StartingDeck;
        if (!PlayerSystem.EnergyEnabled) {
            // Take out energy cards
            startingDeck = startingDeck.Where(x => x.Type != ProgramCardData.CardType.Utility);
        }

        // Cards
        Hand = new CardCollection();
        DrawPile = new CardCollection(startingCards: startingDeck);
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
        CurrentCheckpoint = new ObservableField<int>();
        IsRebooted = new ObservableField<bool>();

        // Initialize
        ExecutionPhase.PhaseEnd += () => {
            IsRebooted.Value = false;
        };
    }

    public void CreateModel(PlayerModel modelPrefab, RebootToken spawnPoint) {
        Model = MapSystem.Instance.CreateObject (
            modelPrefab,
            spawnPoint.GridPos,
            spawnPoint.transform.rotation
        );
        
        Model.Init(this, spawnPoint);
        RobotData.OnSpawn(this);
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

    public void ShuffleDeck() {
        DrawPile.AddRange(DiscardPile.Cards, CardPlacement.Top);
        DiscardPile.Clear();
        DrawPile.Shuffle();
    }

    ProgramCardData GetTopCard() {
        if (DrawPile.Cards.Count == 0) ShuffleDeck();
        var card = DrawPile.Cards[0];
        DrawPile.RemoveCard(0);
        return card;
    }

    const int MaxDrawAttempts = 30;
    
    public ProgramCardData DiscardTopCardsUntil(Func<ProgramCardData, bool> predicate, int maxAttempts = MaxDrawAttempts) {
        for (var i = 0; i < maxAttempts; i++) {
            var card = GetTopCard();
            if (predicate(card)) return card;
            DiscardPile.AddCard(card, CardPlacement.Top);
        }
        Debug.LogError($"Failed to find card matching predicate after {maxAttempts} attempts");
        return null;
    }

    void DrawCard() {
        var card = GetTopCard();

        Hand.AddCard(card, CardPlacement.Top);
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

    void DiscardCard(int index) {
        if (Hand.Cards.Count == 0) return;

        var card = Hand[index];
        Hand.RemoveCard(index);

        DiscardPile.AddCard(card, CardPlacement.Top);
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

    void DiscardRandomCard() {
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
            Program.SetRegister(i, null);
        }
    }
    #endregion

    #region Upgrades

    public IEnumerator GetSlotAndAdd(UpgradeCardData upgrade) {
        var output = new int[1];
        yield return GetUpgradeSlot(output, $"choosing an upgrade to replace with {upgrade}");
        ReplaceUpgradeAt(upgrade, output[0]);
    }

    IEnumerator GetUpgradeSlot(int[] output, string message = "choosing an upgrade to replace") {
        for (var i = 0; i < _upgrades.Length; i++) {
            if (_upgrades[i] != null) continue;
            output[0] = i;
            yield break;
        }
        
        var result = new UpgradeCardData[1];
        yield return ChoiceSystem.DoChoice(new ChoiceData<UpgradeCardData> {
            Overlay = ShopUIController.Instance.OverrideOverlay,
            Player = this,
            Options = _upgrades,
            Message = message,
            OutputArray = result,
            MinChoices = 1
        });
        output[0] = _upgrades.IndexOf(result[0]);
    }
    
    public void ReplaceUpgradeAt(UpgradeCardData upgrade, int index) {
        RemoveUpgrade(index);
        _upgrades[index] = upgrade;
        upgrade.OnAdd(this);
        UpgradeAdded?.Invoke(upgrade, index);
    }

    public void UseUpgrade(UpgradeCardData upgrade) => UseUpgrade(_upgrades.IndexOf(upgrade));

    public void UseUpgrade(int index) {
        var upgrade = _upgrades[index];
        Log.Instance.UseUpgradeMessage(this, upgrade);
        upgrade.Use(this);
        
        UpgradeUsed?.Invoke(upgrade);

        if (upgrade.Type == UpgradeType.Temporary) {
            RemoveUpgrade(index);
        }
    }
    
    public void RemoveUpgrade([NotNull] UpgradeCardData upgrade) => RemoveUpgrade(_upgrades.IndexOf(upgrade));

    void RemoveUpgrade(int index) {
        var upgrade = _upgrades[index];
        if (upgrade == null) return;
        
        upgrade.OnRemove(this);
        _upgrades[index] = null;
        UpgradeRemoved?.Invoke(upgrade, index);
    }
    
    #endregion

    public void OnExecute(ProgramExecution execution) {
        ProgramCardExecuted?.Invoke(execution);
    }
    
    public void Reboot(IBoard board, bool takeDamage = true) {
        board.Respawn(Model);
        if (takeDamage) ApplyCardAffector(RebootAffector);
        
        DiscardHand();
        DiscardProgram();
        
        IsRebooted.Value = true;
        
        Log.Instance.RebootMessage(this);
    }
    
    public void RebootFromParentBoard(bool takeDamage = true) {
        Reboot(MapSystem.GetParentBoard(Model), takeDamage);
    }

    public void ApplyCardAffector(CardAffector affector) {
        var pile = GetCollection(affector.Destination);
        foreach (var card in affector.Cards) {
            pile.AddCard(card, affector.Placement);
        }
        CardAffectorApplied?.Invoke(affector);
    }

    public void SerializeRegisters(out byte playerIndex, out byte[] registers) {
        playerIndex = (byte) PlayerSystem.Players.IndexOf(this);
        registers = Program.Cards.Select(c => (byte) c.GetLookupId()).ToArray();
    }
}

public enum Pile {
    Hand = 2,
    DrawPile = 1,
    DiscardPile = 0
}

public struct PlayerArgs {
    public RobotData RobotData;
    public int StartingEnergy;
    public int CardsPerTurn;
    public int RegisterCount;
    public CardAffector RebootAffector;
    public int UpgradeSlots;
    public string Name;
    public ulong ClientId;
}