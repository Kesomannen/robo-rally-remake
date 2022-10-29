using System.Collections.Generic;
using UnityEngine;

public class HandController : Singleton<HandController> {
    [SerializeField] HandCard _cardPrefab;
    [SerializeField] float _cardSpacing;

    readonly List<HandCard> _cardObjects = new();
    Player _owner => NetworkSystem.LocalPlayer;

    void Start() {
        _owner.Hand.OnAdd += OnCardAdded;
        _owner.Hand.OnRemove += OnCardRemoved;
        CreateHand();    
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

    void DestroyHand() {
        for (int i = 0; i < _cardObjects.Count; i++) {
            DestroyCard(i);
        }
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
            var xPos = _cardSpacing * ((float)i - _cardObjects.Count / 2f + 0.5f);
            var pos = transform.position + Vector3.right * xPos;
            cardObject.SetOrigin(pos, i);
        }
    }
}