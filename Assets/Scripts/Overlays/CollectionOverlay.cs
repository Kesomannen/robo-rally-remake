using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionOverlay : Overlay {
    [SerializeField] Transform _cardContainer;
    [SerializeField] ProgramCard _cardPrefab;
    [SerializeField] DynamicUITween _onEnableTween;

    readonly List<ProgramCard> _cardObjects = new();

    public void Init(IEnumerable<ProgramCardData> cards, bool shuffledView = false) {
        var cardList = cards.ToList();
        if (shuffledView) cardList.Shuffle();
        foreach (var card in cardList) {
            var cardInstance = Instantiate(_cardPrefab, _cardContainer);
            cardInstance.SetContent(card);
            _cardObjects.Add(cardInstance);
            cardInstance.gameObject.SetActive(false);
        }
        StartCoroutine(TweenHelper.DoUITween(_onEnableTween.ToTween(_cardObjects.Select(c => c.gameObject))));
    }
}