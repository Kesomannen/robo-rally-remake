using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : Singleton<HandController> {
    [SerializeField] HandCard _cardPrefab;
    [SerializeField] float _cardSpacing;
    [SerializeField] CanvasScaler _canvasScaler;

    readonly List<HandCard> _cardObjects = new();
    Player _owner => PlayerManager.LocalPlayer;

    void Start() {
        _owner.Hand.OnAdd += OnCardAdded;
        _owner.Hand.OnRemove += OnCardRemoved;
        CreateHand();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        _owner.Hand.OnAdd -= OnCardAdded;
        _owner.Hand.OnRemove -= OnCardRemoved;
    }

    void OnCardAdded(ProgramCardData card, int index) {
        CreateCard(card, index);
    }

    void OnCardRemoved(ProgramCardData card, int index) {
        DestroyCard(index);    
    }

    void DestroyCard(int index) {
        var card = _cardObjects[index];
        _cardObjects.RemoveAt(index);
        Destroy(card.gameObject);

        UpdateCards();
    }

    ProgramCard CreateCard(ProgramCardData card, int index) {
        var cardObject = Instantiate(_cardPrefab, transform);

        cardObject.SetData(card);
        _cardObjects.Insert(index, cardObject);

        UpdateCards();
        return cardObject;
    }

    void CreateHand() {
        for (int i = 0; i < _owner.Hand.Cards.Count; i++) {
            var card = _owner.Hand.Cards[i];
            CreateCard(card, i);
        }
    }

    void UpdateCards() {
        for (int i = 0; i < _cardObjects.Count; i++) {
            var cardObject = _cardObjects[i];
            var xPos = _cardSpacing * ((float)i - (float)_cardObjects.Count / 2f + 0.5f) * _canvasScaler.transform.localScale.x;
            var pos = transform.position + Vector3.right * xPos;
            cardObject.SetOrigin(pos, i);
        }
    }
}