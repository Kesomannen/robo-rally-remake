using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class CardCollection {
    readonly List<ProgramCardData> _cards;

    public IReadOnlyList<ProgramCardData> Cards => _cards;
    public ProgramCardData this[int index] => Cards[index];

    int _maxCards;

    public int MaxCards
    {
        get => _maxCards;
        set
        {
            _maxCards = value;
            if (_cards.Count > _maxCards)
            {
                _cards.RemoveRange(_maxCards, _cards.Count - _maxCards);
            }
        }
    }

    public event Action OnShuffle;
    public delegate void CardUpdateHandler(ProgramCardData card, int index);
    public event CardUpdateHandler OnAdd, OnRemove;

    public CardCollection(IEnumerable<ProgramCardData> startingCards = null, int maxCards = int.MaxValue)
    {
        if (startingCards != null) _cards = startingCards.ToList();
        else _cards = new List<ProgramCardData>();
        _maxCards = maxCards;
    }

    public void Shuffle()
    {
        _cards.Shuffle();
        OnShuffle?.Invoke();
    }

    public bool AddCard(ProgramCardData card, CardPlacement placement)
    {
        if (_cards.Count >= _maxCards) return false;
        var index = GetIndex(placement);
        _cards.Insert(index, card);
        OnAdd?.Invoke(card, index);
        return true;
    }

    public int AddRange(IReadOnlyList<ProgramCardData> cards, CardPlacement placement)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (!AddCard(cards[i], placement)) return i;
        }
        return cards.Count;
    }

    public bool RemoveCard(ProgramCardData card)
    {
        var successful = _cards.Remove(card);
        return successful;
    }

    public bool RemoveCard(int index)
    {
        if (index < 0 || index >= _cards.Count) return false;
        var card = _cards[index];
        _cards.RemoveAt(index);
        OnRemove?.Invoke(card, index);
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            RemoveCard(i);
        }
    }

    public int GetIndex(CardPlacement placement)
    {
        return placement switch
        {
            CardPlacement.Top => 0,
            CardPlacement.Bottom => _cards.Count - 1,
            CardPlacement.Random => Random.Range(0, _cards.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    public ProgramCardData GetCard(CardPlacement placement)
    {
        return _cards[GetIndex(placement)];
    }
}

public enum CardPlacement {
    Top,
    Bottom,
    Random,
}