using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class ShopPhase : NetworkSingleton<ShopPhase> {
    static int _skippedPlayers;
    static bool _currentPlayerReady;

    static UpgradeCardData[] _shopCards;
    public static IReadOnlyList<UpgradeCardData> ShopCards => _shopCards;

    [CanBeNull] public static Player CurrentPlayer { get; private set; }
    
    const float RestockDelay = 0.5f;

    public static event Action<Player, bool, UpgradeCardData> OnPlayerDecision;
    public static event Action<int, UpgradeCardData> OnRestock;

    protected override void Awake() {
        base.Awake();
        _shopCards = new UpgradeCardData[GameSettings.Instance.ShopSlots];
    }

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.ChangeState(UIState.Shop);
        
        yield return RestockRoutine();

        foreach (var player in PlayerManager.Players) {
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
            
            _shopCards[i] = UpgradeCardData.GetRandom();
            Debug.Log($"Restocked {i} with {_shopCards[i].Name}");
            OnRestock?.Invoke(i, _shopCards[i]);

            yield return CoroutineUtils.Wait(RestockDelay);
        }
        yield return Scheduler.WaitUntilClearRoutine();
    }

    public void MakeDecision(bool skipped, UpgradeCardData upgrade, int index) {
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
        if (PlayerManager.IsLocal(player)) return;
        
        if (skipped){
            _skippedPlayers++;
            OnPlayerDecision?.Invoke(player, true, null);
        } else{
            var upgrade = UpgradeCardData.GetById(upgradeID);
            player.BuyUpgrade(upgrade, index);
            OnPlayerDecision?.Invoke(player, false, upgrade);
        }
        _currentPlayerReady = true;
    }
}