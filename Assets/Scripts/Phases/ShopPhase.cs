using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopPhase : NetworkSingleton<ShopPhase> {
    static int _skippedPlayers;
    static bool _currentPlayerReady;

    static UpgradeCardData[] _shopCards;
    public static IReadOnlyList<UpgradeCardData> ShopCards => _shopCards;
    
    static List<UpgradeCardData> _availableCards;

    [CanBeNull] public static Player CurrentPlayer { get; private set; }

    const float RestockDelay = 0.5f;

    public static event Action<Player, bool, UpgradeCardData> OnPlayerDecision;
    public static event Action<Player> OnNewPlayer; 
    public static event Action<int, UpgradeCardData> OnRestock;
    public static event Action OnPhaseStarted;

    protected override void Awake() {
        base.Awake();
        _shopCards = new UpgradeCardData[GameSettings.Instance.ShopSlots];
    }

    public IEnumerator DoPhase() {
        UIManager.Instance.ChangeState(UIState.Shop);
        OnPhaseStarted?.Invoke();

        _restockTrigger = false;
        RestockCards();
        
        yield return new WaitUntil(() => _restockTrigger);

        var orderedPlayers = PlayerSystem.GetOrderedPlayers();

        _skippedPlayers = 0;
        foreach (var player in orderedPlayers) {
            Debug.Log($"Shop phase for {player}");
            CurrentPlayer = player;
            OnNewPlayer?.Invoke(player);
            _currentPlayerReady = false;
            
            yield return new WaitUntil(() => _currentPlayerReady);
        }
        CurrentPlayer = null;

        if (_skippedPlayers != PlayerSystem.Players.Count) yield break;
        for (var i = 0; i < _shopCards.Length; i++) {
            _shopCards[i] = null;
        }
        RestockCards();
    }

    static bool _restockTrigger;
    
    [ClientRpc]
    void RestockClientRpc(byte[] slots, byte[] cardIds) {
        StartCoroutine(RestockRoutine(
            slots.Select(x => (int)x).ToArray(),
            cardIds.Select(x => UpgradeCardData.GetById(x)).ToArray()
            ));
    }

    static IEnumerator RestockRoutine(IReadOnlyCollection<int> slots, IReadOnlyList<UpgradeCardData> cards) {
        for (var i = 0; i < slots.Count; i++) {
            _shopCards[i] = cards[i];
            OnRestock?.Invoke(i, cards[i]);
            yield return CoroutineUtils.Wait(RestockDelay);   
        }
        _restockTrigger = true;
    }

    void RestockCards() {
        if (NetworkManager != null && !IsServer) return;
        
        // Randomize shop and send to clients
        var slots = new List<byte>();
        var cardIds = new List<byte>();

        for (var i = 0; i < _shopCards.Length; i++) {
            Debug.Log($"Restocking slot {i}");
            var card = _shopCards[i];
            if (card != null) continue;
                
            if (_availableCards == null || _availableCards.Count == 0) {
                _availableCards = UpgradeCardData.GetAll().ToList();
            }
            
            var randomIndex = Random.Range(0, _availableCards!.Count);
            var newCard = _availableCards[randomIndex];
            _availableCards.RemoveAt(randomIndex);
                
            slots.Add((byte) i);
            cardIds.Add((byte) newCard.GetLookupId());

            if (IsClient) continue;
            _shopCards[i] = newCard;
            OnRestock?.Invoke(i, newCard);
        }
            
        RestockClientRpc(slots.ToArray(), cardIds.ToArray());
    }

    public void MakeDecision(bool skipped, UpgradeCardData upgrade, int index) {
        var id = skipped ? 0 : upgrade.GetLookupId();

        if (NetworkManager == null) {
            SetReady(
                PlayerSystem.LocalPlayer,
                skipped,
                upgrade,
                index
);
        } else {
            MakeDecisionServerRpc (
                (byte) PlayerSystem.Players.IndexOf(PlayerSystem.LocalPlayer),
                skipped,
                (byte) id,
                (byte) index
            );   
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void MakeDecisionServerRpc(byte playerIndex, bool skipped, byte upgradeID, byte index) {
        SetReady(PlayerSystem.Players[playerIndex], skipped, UpgradeCardData.GetById(upgradeID), index);
        MakeDecisionClientRpc(playerIndex, skipped, upgradeID, index);
    }
    
    [ClientRpc]
    void MakeDecisionClientRpc(byte playerIndex, bool skipped, byte upgradeID, byte index) {
        if (IsServer) return;
        SetReady(PlayerSystem.Players[playerIndex], skipped, UpgradeCardData.GetById(upgradeID), index);
    }
    
    static void SetReady(Player player, bool skipped, UpgradeCardData upgrade, int playerUpgradeIndex) {
        if (skipped) {
            _skippedPlayers++;
            
            OnPlayerDecision?.Invoke(player, true, null);
        } else {
            player.Energy.Value -= upgrade.Cost;
            player.AddUpgrade(upgrade, playerUpgradeIndex);
            _shopCards[_shopCards.IndexOf(upgrade)] = null;
            
            OnPlayerDecision?.Invoke(player, false, upgrade);
        }
        _currentPlayerReady = true;
    }
}