using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopPhase : NetworkSingleton<ShopPhase> {
    UpgradeCardData[] _shopCards;
    UpgradeCardData[] _restockCards;
    List<UpgradeCardData> _cardsInDeck;
    UpgradeCardData[] _availableCards;
    
    int _skippedPlayers;
    bool _currentPlayerReady;
    
    public Player CurrentPlayer { get; private set; }
    public IReadOnlyList<UpgradeCardData> ShopCards => _shopCards;

    public static event Action<Player, bool, UpgradeCardData> PlayerDecision;
    public static event Action<Player> NewPlayer;
    public static event Action<int, UpgradeCardData> Restock;

    public static event Action PhaseStarted;

    public void Initialize(IEnumerable<UpgradeCardData> availableCards) {
        _availableCards = availableCards.ToArray();
    }
    
    public IEnumerator DoPhase() {
        _shopCards ??= new UpgradeCardData[PlayerSystem.Players.Count];
        
        yield return UIManager.Instance.ChangeState(UIState.Shop);
        PhaseStarted?.Invoke();

        yield return RestockCards(true);
        yield return TaskScheduler.WaitUntilClear();

        _skippedPlayers = 0;
        var orderedPlayers = PlayerSystem.Instance.GetOrderedPlayers();
        foreach (var player in orderedPlayers) {
            yield return DoPlayerTurn(player);
            yield return TaskScheduler.WaitUntilClear();
        }

        CurrentPlayer = null;
        NewPlayer?.Invoke(null);

        if (_skippedPlayers != PlayerSystem.Players.Count) yield break;
        Log.Message("The shop was fully restocked due to all players skipping their turn");
        yield return RestockCards(false);
        yield return TaskScheduler.WaitUntilClear();
    }

    IEnumerator DoPlayerTurn(Player player) {
        CurrentPlayer = player;
        NewPlayer?.Invoke(player);
        
        yield return new WaitUntil(() => _currentPlayerReady);
        _currentPlayerReady = false;
    }

    UpgradeCardData DrawRandomCard() {
        UpgradeCardData card;
        do {
            if (_cardsInDeck == null || _cardsInDeck.Count == 0) {
                _cardsInDeck = new List<UpgradeCardData>(_availableCards);
            }
            
            var randomIndex = Random.Range(0, _cardsInDeck.Count);
            card = _cardsInDeck[randomIndex];
            _cardsInDeck.RemoveAt(randomIndex);
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
    
    void SetReady(bool skipped, UpgradeCardData upgrade, int playerUpgradeIndex) {
        if (skipped) {
            _skippedPlayers++;
            
            PlayerDecision?.Invoke(CurrentPlayer, true, null);
            Log.Message($"{Log.PlayerString(CurrentPlayer)} skipped buying an upgrade");
        } else {
            CurrentPlayer.Energy.Value -= upgrade.Cost;
            CurrentPlayer.ReplaceUpgradeAt(upgrade, playerUpgradeIndex);
            _shopCards[_shopCards.IndexOf(upgrade)] = null;
            
            PlayerDecision?.Invoke(CurrentPlayer, false, upgrade);
            Log.Message($"{Log.PlayerString(CurrentPlayer)} bought {Log.UpgradeString(upgrade)} for {Log.EnergyString(upgrade.Cost)}");
        }
        CurrentPlayer = null;
        _currentPlayerReady = true;
    }
}