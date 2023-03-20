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
    public static IEnumerable<UpgradeCardData> ShopCards => _shopCards;

    public static event Action<Player, bool, UpgradeCardData> PlayerDecision;
    public static event Action<Player> NewPlayer;
    public static event Action<int, UpgradeCardData> Restock;

    public static event Action PhaseStarted;

    protected override void Awake() {
        base.Awake();
        _shopCards = new UpgradeCardData[LobbySystem.LobbySettings.ShopCards];
    }

    public IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Shop);
        PhaseStarted?.Invoke();

        yield return RestockCards(true);
        yield return TaskScheduler.WaitUntilClear();

        _skippedPlayers = 0;
        var orderedPlayers = PlayerSystem.GetOrderedPlayers();
        foreach (var player in orderedPlayers) {
            yield return DoPlayerTurn(player);
            yield return TaskScheduler.WaitUntilClear();
        }

        CurrentPlayer = null;
        NewPlayer?.Invoke(null);

        if (_skippedPlayers != PlayerSystem.Players.Count) yield break;
        yield return RestockCards(false);
        yield return TaskScheduler.WaitUntilClear();
    }

    static IEnumerator DoPlayerTurn(Player player) {
        CurrentPlayer = player;
        NewPlayer?.Invoke(player);
        
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
        if (IsServer) {
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

        for (var i = 0; i < _restockCards.Length; i++) {
            if (_shopCards[i] == _restockCards[i]) continue;
            
            var card = _restockCards[i];
            _shopCards[i] = card;
            Restock?.Invoke(i, card);
        }
        _restockCards = null;
    }

    [ClientRpc]
    void RestockCardsClientRpc(byte[] cardIds) {
        if (IsServer) return;
        _restockCards = cardIds.Select(c => UpgradeCardData.GetById(c)).ToArray();
    }
    
    public void MakeDecision(bool skipped, UpgradeCardData upgrade, int index) {
        var id = skipped ? 0 : upgrade.GetLookupId();
        MakeDecisionServerRpc(skipped, (byte)id, (byte)index);
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
            
            PlayerDecision?.Invoke(CurrentPlayer, true, null);
            Log.Instance.SkipMessage(CurrentPlayer);
        } else {
            CurrentPlayer.Energy.Value -= upgrade.Cost;
            CurrentPlayer.ReplaceUpgradeAt(upgrade, playerUpgradeIndex);
            _shopCards[_shopCards.IndexOf(upgrade)] = null;
            
            PlayerDecision?.Invoke(CurrentPlayer, false, upgrade);
            Log.Instance.BuyUpgradeMessage(CurrentPlayer, upgrade);
        }
        CurrentPlayer = null;
        _currentPlayerReady = true;
    }
}