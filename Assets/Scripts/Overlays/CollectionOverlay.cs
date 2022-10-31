using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionOverlay : OverlayBase {
    [SerializeField] ProgramCard _cardPrefab;
    [SerializeField] Transform _cardContainer;

    public void Init(IEnumerable<ProgramCardData> cards, bool shuffled = false) {
        var cardList = cards.ToList();
        if (shuffled) cardList.Shuffle();
        foreach (var card in cardList) {
            var cardInstance = Instantiate(_cardPrefab, _cardContainer);
            cardInstance.SetData(card);
        }
    }
}