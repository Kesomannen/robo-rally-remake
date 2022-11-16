using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : Singleton<HandController> {
    [Header("References")]
    [SerializeField] HandCard _cardPrefab;
    [SerializeField] RectTransform _drawPile, discardPile;

    [Header("Animation")]
    [SerializeField] float _cardSpacing;
    [SerializeField] LeanTweenType _easingType;
    [SerializeField] float _cardMoveDuration;
    [SerializeField] float _drawDiscardDelay;

    readonly List<HandCard> _cardObjects = new();
    readonly List<CardAction> _actionQueue = new();

    Player Owner => PlayerManager.LocalPlayer;

    void Start() {
        Owner.Hand.OnAdd += OnCardAdded;
        Owner.Hand.OnRemove += OnCardRemoved;
        Owner.OnDraw += OnDraw;
        Owner.OnDiscard += OnDiscard;
        CreateHand();
    }

    void OnEnable() {
        StartCoroutine(UpdateHandRoutine());
    }

    IEnumerator UpdateHandRoutine() {
        while (true) {
            yield return Helpers.WaitEndOfFrame();
            if (_actionQueue.Count > 0) {
                var action = _actionQueue[0];
                _actionQueue.RemoveAt(0);
                switch (action.Type) {
                    case CardActionType.Draw: yield return DrawCardRoutine(action); break;
                    case CardActionType.Discard: yield return DiscardCardRoutine(action); break;
                    case CardActionType.Add: yield return AddCardRoutine(action); break;
                    case CardActionType.Remove: yield return RemoveCardRoutine(action); break;
                }
                yield return Helpers.Wait(action.Delay);
            }
        }
    }

    IEnumerator DrawCardRoutine(CardAction a) {
        var cardObject = CreateCard(a.Card, a.Index);
        cardObject.transform.position = _drawPile.position;
        var tween = LeanTween.move(cardObject.gameObject, GetOrigin(a.Index), _cardMoveDuration)
                             .setEase(_easingType);
        yield return tween;

        cardObject.enabled = true;
    }

    IEnumerator DiscardCardRoutine(CardAction a) {
        var cardObject = _cardObjects[a.Index];
        cardObject.enabled = false;

        DestroyCard(a.Index);
        yield break;
    }

    IEnumerator AddCardRoutine(CardAction a) {
        var cardObject = CreateCard(a.Card, a.Index);

        cardObject.enabled = true;
        yield break;
    }

    IEnumerator RemoveCardRoutine(CardAction a) {
        var cardObject = _cardObjects[a.Index];
        cardObject.enabled = false;

        DestroyCard(a.Index);
        yield break;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        Owner.Hand.OnAdd -= OnCardAdded;
        Owner.Hand.OnRemove -= OnCardRemoved;
        Owner.OnDraw -= OnDraw;
        Owner.OnDiscard -= OnDiscard;
    }

    void OnCardAdded(ProgramCardData card, int index) {
        _actionQueue.Add(new CardAction {
            Type = CardActionType.Add,
            Card = card,
            Index = index,
        });
    }

    void OnCardRemoved(ProgramCardData card, int index) {
        _actionQueue.Add(new CardAction {
            Type = CardActionType.Remove,
            Card = card,
            Index = index,
        });
    }

    void OnDraw(ProgramCardData card) {
        // OnDraw gets called right after OnCardAdded, so we modify the last action
        var index = _actionQueue.Count - 1;
        var lastAction = _actionQueue[index];
        lastAction.Type = CardActionType.Draw;
        lastAction.Delay = _drawDiscardDelay;
        _actionQueue[index] = lastAction;
    }

    void OnDiscard(ProgramCardData card) {
        // OnDiscard gets called right after OnCardRemoved, so we modify the last action
        var index = _actionQueue.Count - 1;
        var lastAction = _actionQueue[index];
        lastAction.Type = CardActionType.Discard;
        lastAction.Delay = _drawDiscardDelay;
        _actionQueue[index] = lastAction;
    }

    void DestroyCard(int index) {
        var card = _cardObjects[index];
        _cardObjects.RemoveAt(index);
        Destroy(card.gameObject);

        UpdateCards();
    }

    HandCard CreateCard(ProgramCardData card, int index) {
        var cardObject = Instantiate(_cardPrefab, transform);
        cardObject.enabled = false;

        cardObject.SetData(card);
        _cardObjects.Insert(index, cardObject);

        UpdateCards();
        return cardObject;
    }

    void CreateHand() {
        for (int i = 0; i < Owner.Hand.Cards.Count; i++) {
            var card = Owner.Hand.Cards[i];
            _actionQueue.Add(new CardAction {
                Type = CardActionType.Draw,
                Card = card,
                Index = i,
            });
        }
    }

    void UpdateCards() {
        for (int i = 0; i < _cardObjects.Count; i++) {
            var cardObject = _cardObjects[i];
            cardObject.SetOrigin(GetOrigin(i), i);
        }
    }

    Vector3 GetOrigin(int index) {
        var xPos = _cardSpacing * ((float)index - (float)_cardObjects.Count / 2f + 0.5f);
        var pos = transform.position + CanvasUtils.Scale.x * xPos * Vector3.right;
        return pos;
    }

    struct CardAction {
        public ProgramCardData Card;
        public CardActionType Type;
        public int Index;
        public float Delay;
    }

    enum CardActionType {
        Add,
        Remove,
        Draw,
        Discard,
    }
}