using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionOverlay : OverlayBase {
    [Header("References")]
    [SerializeField] ProgramCard _cardPrefab;
    [SerializeField] Transform _cardContainer;
    [Header("Animation")]
    [SerializeField] float _cardPopupDuration = 0.2f;
    [SerializeField] float _cardPopupDelay = 0.05f;
    [SerializeField] float _cardStartScale = 0.5f;
    [SerializeField] LeanTweenType _easeType;

    readonly List<ProgramCard> _cardObjects = new();

    public void Init(IEnumerable<ProgramCardData> cards, bool shuffledView = false) {
        var cardList = cards.ToList();
        if (shuffledView) cardList.Shuffle();
        foreach (var card in cardList) {
            var cardInstance = Instantiate(_cardPrefab, _cardContainer);
            cardInstance.SetData(card);
            _cardObjects.Add(cardInstance);
            cardInstance.gameObject.SetActive(false);
        }
        StartCoroutine(ShowCardsRoutine());

        IEnumerator ShowCardsRoutine() {
            foreach (var card in _cardObjects) {
                var obj = card.gameObject;
                obj.transform.localScale = Vector3.one * _cardStartScale;
                LeanTween.scale(obj, Vector3.one, _cardPopupDuration).setEase(_easeType);
                obj.SetActive(true);
                yield return Helpers.Wait(_cardPopupDelay);
            }
        }
    }
}