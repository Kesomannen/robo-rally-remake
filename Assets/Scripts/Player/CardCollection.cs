using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

public class CardCollection {
    readonly List<ProgramCardData> _cards;

    public IReadOnlyList<ProgramCardData> Cards => _cards;
    public ProgramCardData this[int index] => Cards[index];

    int _maxCards;

    public int MaxCards {
        get => _maxCards;
        set {
            _maxCards = value;
            if (_cards.Count > _maxCards) {
                _cards.RemoveRange(_maxCards, _cards.Count - _maxCards);
            }
        }
    }

    public event Action OnShuffle;
    public delegate void CardUpdateHandler(ProgramCardData card, int index);
    public event CardUpdateHandler OnAdd, OnRemove;

    public CardCollection(IEnumerable<ProgramCardData> startingCards = null, int maxCards = int.MaxValue){
        _cards = startingCards != null ? startingCards.ToList() : new List<ProgramCardData>();
        _maxCards = maxCards;
    }

    public void Shuffle() {
        _cards.Shuffle();
        OnShuffle?.Invoke();
    }

    public bool AddCard([NotNull] ProgramCardData card, CardPlacement placement) {
        if (_cards.Count >= _maxCards) return false;

        var index = GetIndex(placement);
        _cards.Insert(index, card);
        OnAdd?.Invoke(card, index);
        return true;
    }

    public void AddRange(IEnumerable<ProgramCardData> cards, CardPlacement placement) {
        if (cards.Any(t => !AddCard(t, placement))) {
            throw new InvalidOperationException("Not enough space in collection");
        }
    }

    public void RemoveCard(ProgramCardData card) {
        var index = _cards.IndexOf(card);
        if (index == -1) return;
        RemoveCard(index);
    }

    public void RemoveCard(int index) {
        var card = _cards[index];
        _cards.RemoveAt(index);
        OnRemove?.Invoke(card, index);
    }

    public void Clear() {
        var cards = _cards.Count;
        for (var i = 0; i < cards; i++) {
            RemoveCard(Cards.Count - 1);
        }
    }

    int GetIndex(CardPlacement placement) {
        return placement switch {
            CardPlacement.Top => 0,
            CardPlacement.Bottom => _cards.Count - 1,
            CardPlacement.Random => Random.Range(0, _cards.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    public ProgramCardData GetCard(CardPlacement placement) {
        return _cards[GetIndex(placement)];
    }
}

public enum CardPlacement {
    Top = 0,
    Bottom = 1,
    Random = 2,
}