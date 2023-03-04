using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopPhase : NetworkSingleton<ShopPhase> {
    static UpgradeCardData[] _shopCards;
    static UpgradeCardData[] _restockCards;
    static List<UpgradeCardData> _availableCards;
    static int _skippedPlayers;
    static bool _currentPlayerReady;
    
    public static Player CurrentPlayer { get; private set; }

    const float RestockDelay = 0.25f;
    
    public static event Action<Player, bool, UpgradeCardData> OnPlayerDecision;
    public static event Action<Player> OnNewPlayer;
    public static event Action<int, UpgradeCardData> OnRestock;

    public static event Action OnPhaseStarted;

    protected override void Awake() {
        base.Awake();
        _shopCards = new UpgradeCardData[GameSettings.Instance.ShopSlots];
    }

    public IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Shop);
        OnPhaseStarted?.Invoke();

        yield return RestockCards(true);
        yield return TaskScheduler.WaitUntilClear();

        _skippedPlayers = 0;
        var orderedPlayers = PlayerSystem.GetOrderedPlayers();
        TaskScheduler.PushSequence(routines: orderedPlayers.Select(DoPlayerTurn).ToArray());
        yield return TaskScheduler.WaitUntilClear();
        
        CurrentPlayer = null;
        OnNewPlayer?.Invoke(null);

        if (_skippedPlayers != PlayerSystem.Players.Count) yield break;
        yield return RestockCards(false);
        yield return TaskScheduler.WaitUntilClear();
    }

    static IEnumerator DoPlayerTurn(Player player) {
        CurrentPlayer = player;
        OnNewPlayer?.Invoke(player);
        
        yield return new WaitUntil(() => _currentPlayerReady);
        _currentPlayerReady = false;
    }

    static UpgradeCardData DrawRandomCard() {
        if (_availableCards == null || _availableCards.Count == 0) {
            _availableCards = UpgradeCardData.GetAll().ToList();
        }
        
        UpgradeCardData card;
        do {
            var randomIndex = Random.Range(0, _availableCards.Count);
            card = _availableCards[randomIndex];
            _availableCards.RemoveAt(randomIndex);
        } while (_shopCards.Contains(card));
        
        return card;
    }
    
    IEnumerator RestockCards(bool onlyIfEmpty) {
        if (IsServer || NetworkManager == null) {
            _restockCards = new UpgradeCardData[_shopCards.Length];
            for (var i = 0; i < _shopCards.Length; i++) {
                 if (_shopCards[i] == null || !onlyIfEmpty) {
                     _restockCards[i] = DrawRandomCard();
                 } else {
                     _restockCards[i] = _shopCards[i];
                 }
            }
            
            RestockCardsClientRpc(_restockCards.Select(c => (byte) c.GetLookupId()).ToArray());
        } else {
            yield return new WaitUntil(() => _restockCards != null);
        }

        for (var i = _restockCards!.Length - 1; i >= 0; i--) {
            Debug.Log($"Restocking card {i} with {_restockCards[i]}");
            if (_shopCards[i] == _restockCards[i] && _shopCards[i] != null) continue;
            TaskScheduler.PushRoutine(Restock(i, _restockCards[i]));
        }
        _restockCards = null;
        
        IEnumerator Restock(int index, UpgradeCardData card) {
            yield return CoroutineUtils.Wait(RestockDelay);
            _shopCards[index] = card;
            OnRestock?.Invoke(index, card);
        }
    }

    [ClientRpc]
    void RestockCardsClientRpc(byte[] cardIds) {
        if (IsServer) return;
        _restockCards = cardIds.Select(c => UpgradeCardData.GetById(c)).ToArray();
    }
    
    public void MakeDecision(bool skipped, UpgradeCardData upgrade, int index) {
        var id = skipped ? 0 : upgrade.GetLookupId();

        if (NetworkManager == null) {
            SetReady(skipped, upgrade, index);
        } else {
            MakeDecisionServerRpc(skipped, (byte) id, (byte) index);    
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void MakeDecisionServerRpc(bool skipped, byte upgradeID, byte index) {
        SetReady(skipped, UpgradeCardData.GetById(upgradeID), index);
        MakeDecisionClientRpc(skipped, upgradeID, index);
    }
    
    [ClientRpc]
    void MakeDecisionClientRpc(bool skipped, byte upgradeID, byte index) {
        if (IsServer) return;
        SetReady(skipped, UpgradeCardData.GetById(upgradeID), index);
    }
    
    static void SetReady(bool skipped, UpgradeCardData upgrade, int playerUpgradeIndex) {
        if (skipped) {
            _skippedPlayers++;
            
            OnPlayerDecision?.Invoke(CurrentPlayer, true, null);
            Log.Instance.SkipMessage(CurrentPlayer);
        } else {
            CurrentPlayer.Energy.Value -= upgrade.Cost;
            CurrentPlayer.ReplaceUpgradeAt(upgrade, playerUpgradeIndex);
            _shopCards[_shopCards.IndexOf(upgrade)] = null;
            
            OnPlayerDecision?.Invoke(CurrentPlayer, false, upgrade);
            Log.Instance.BuyUpgradeMessage(CurrentPlayer, upgrade);
        }
        _currentPlayerReady = true;
    }
}