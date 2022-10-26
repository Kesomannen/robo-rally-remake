using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class CardCollection {
    readonly List<ProgramCardData> _cards;

    public IReadOnlyList<ProgramCardData> Cards => _cards;
    public ProgramCardData this[int index] => _cards[index];

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

    public CardCollection(IEnumerable<ProgramCardData> cards = null, int maxCards = int.MaxValue) {
        _cards = cards.ToList() ?? new();
        _maxCards = maxCards;
    }

    public void Shuffle() {
        for (int i = _cards.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            var temp = _cards[i];
            _cards[i] = _cards[j];
            _cards[j] = temp;
        }
    }

    public bool AddCard(ProgramCardData card, CardPlacement placement) {
        if (_cards.Count >= _maxCards) return false;
        _cards.Insert(GetIndex(placement), card);
        return true;
    }

    public int GetIndex(CardPlacement placement) {
        return placement switch {
            CardPlacement.Top => 0,
            CardPlacement.Bottom => _cards.Count - 1,
            CardPlacement.Random => Random.Range(0, _cards.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }
}

public enum CardPlacement {
    Top,
    Bottom,
    Random,
}