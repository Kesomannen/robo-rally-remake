using System.Collections.Generic;
using UnityEngine;

public class HandController : Singleton<HandController> {
    [SerializeField] HandCard _cardPrefab;

    readonly List<HandCard> _cardObjects = new();
    Player _owner => NetworkSystem.LocalPlayer;

    protected override void Awake() {
        base.Awake();
        PlayerManager.OnPlayersCreated += _ => {
            _owner.Hand.OnAdd += OnCardAdded;
            _owner.Hand.OnRemove += OnCardRemoved;
        };
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
    }

    void DestroyHand() {
        for (int i = 0; i < _cardObjects.Count; i++) {
            DestroyCard(i);
        }
    }

    ProgramCard CreateCard(ProgramCardData card, int index) {
        var cardObject = Instantiate(_cardPrefab, transform);
        cardObject.SetData(card);
        cardObject.transform.SetSiblingIndex(index);
        _cardObjects.Insert(index, cardObject);
        return cardObject;
    }

    void CreateHand() {
        for (int i = 0; i < _owner.Hand.Cards.Count; i++) {
            var card = _owner.Hand.Cards[i];
            CreateCard(card, i);
        }
    }
}