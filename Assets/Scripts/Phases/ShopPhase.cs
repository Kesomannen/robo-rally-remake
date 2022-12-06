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
    public static IReadOnlyList<UpgradeCardData> AvailableCards => _availableCards;

    [CanBeNull] public static Player CurrentPlayer { get; private set; }
    
    const float RestockDelay = 0.5f;

    public static event Action<Player, bool, UpgradeCardData> OnPlayerDecision;
    public static event Action<int, UpgradeCardData> OnRestock;

    protected override void Awake() {
        base.Awake();
        _shopCards = new UpgradeCardData[GameSettings.Instance.ShopSlots];
        _availableCards = UpgradeCardData.GetAll().ToList();
    }

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.ChangeState(UIState.Shop);
        
        yield return RestockRoutine();

        var orderedPlayers = PlayerManager.GetOrderedPlayers();

        foreach (var player in orderedPlayers) {
            Debug.Log($"Shop phase for {player}");
            CurrentPlayer = player;
            _currentPlayerReady = false;
            yield return new WaitUntil(() => _currentPlayerReady);
        }
        CurrentPlayer = null;
        
        if (_skippedPlayers == PlayerManager.Players.Count) {
            for (var i = 0; i < _shopCards.Length; i++){
                _shopCards[i] = null;
            }
            yield return RestockRoutine();
        }
        
        yield return Scheduler.WaitUntilClearRoutine();
    }

    static IEnumerator RestockRoutine() {
        for (var i = 0; i < _shopCards.Length; i++){
            var card = _shopCards[i];
            if (card != null) continue;

            var randomIndex = Random.Range(0, _availableCards.Count);
            _shopCards[i] = _availableCards[randomIndex];
            _availableCards.RemoveAt(randomIndex);
            
            Debug.Log($"Restocked {i} with {_shopCards[i].Name}");
            OnRestock?.Invoke(i, _shopCards[i]);

            yield return CoroutineUtils.Wait(RestockDelay);
        }
        yield return Scheduler.WaitUntilClearRoutine();
    }

    public void MakeDecision(bool skipped, UpgradeCardData upgrade, int index) {
        if (NetworkManager.Singleton == null){
            SetReady((byte) PlayerManager.Players.IndexOf(PlayerManager.LocalPlayer),
                skipped,
                (byte) upgrade.GetLookupId(),
                (byte) index);
        }
        
        MakeDecisionServerRpc (
            (byte) PlayerManager.Players.IndexOf(PlayerManager.LocalPlayer),
            skipped,
            (byte) upgrade.GetLookupId(),
            (byte) index
        );
    }
    
    [ServerRpc(RequireOwnership = false)]
    void MakeDecisionServerRpc(byte playerIndex, bool skipped, byte upgradeID, byte index) {
        SetReady(playerIndex, skipped, upgradeID, index);
        MakeDecisionClientRpc(playerIndex, skipped, upgradeID, index);
    }
    
    [ClientRpc]
    void MakeDecisionClientRpc(byte playerIndex, bool skipped, byte upgradeID, byte index) {
        if (IsServer) return;
        SetReady(playerIndex, skipped, upgradeID, index);
    }
    
    static void SetReady(byte playerIndex, bool skipped, byte upgradeID, byte index) {
        var player = PlayerManager.Players[playerIndex];

        if (skipped){
            _skippedPlayers++;
            
            OnPlayerDecision?.Invoke(player, true, null);
        } else{
            var upgrade = UpgradeCardData.GetById(upgradeID);
            player.Energy.Value -= upgrade.Cost;
            player.BuyUpgrade(upgrade, index);
            _shopCards[_shopCards.IndexOf(upgrade)] = null;
            
            OnPlayerDecision?.Invoke(player, false, upgrade);
        }
        _currentPlayerReady = true;
    }
}