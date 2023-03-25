using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HandController : Singleton<HandController> {
    [Header("References")]
    [FormerlySerializedAs("_cardPrefab")]
    [SerializeField] HandProgramCard _programCardPrefab;
    [SerializeField] RectTransform _drawPile;
    [SerializeField] Transform _highlightParent;
    
    [Header("Animation")]
    [SerializeField] float _maxCardSpacing;
    [SerializeField] float _maxSize;
    [SerializeField] float _cardVerticalOffset;
    [SerializeField] LeanTweenType _easingType;
    [SerializeField] float _cardMoveDuration;
    [SerializeField] float _drawDiscardDelay;

    readonly List<HandProgramCard> _cardObjects = new();
    readonly List<CardAction> _actionQueue = new();

    static Player Owner => PlayerSystem.LocalPlayer;
    float CardSpacing
            => _maxCardSpacing * _cardObjects.Count > _maxSize 
            ? _maxSize / _cardObjects.Count 
            : _maxCardSpacing;

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
            yield return CoroutineUtils.WaitEndOfFrame();
            if (_actionQueue.Count <= 0) continue;
            
            var action = _actionQueue[0];
            _actionQueue.RemoveAt(0);
            yield return action.Type switch {
                CardActionType.Draw => DrawCardRoutine(action),
                CardActionType.Discard => DiscardCardRoutine(action),
                CardActionType.Add => AddCardRoutine(action),
                CardActionType.Remove => RemoveCardRoutine(action),
                _ => throw new ArgumentOutOfRangeException()
            };
            yield return CoroutineUtils.Wait(action.Delay);
        }
    }

    IEnumerator DrawCardRoutine(CardAction a) {
        var cardObject = CreateCard(a.Card, a.Index);
        cardObject.transform.position = _drawPile.position;
        LeanTween
            .move(cardObject.gameObject, GetOriginAndOffset(a.Index).Origin, _cardMoveDuration)
            .setEase(_easingType);
        yield return CoroutineUtils.Wait(_cardMoveDuration);

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

    HandProgramCard CreateCard(ProgramCardData card, int index) {
        var cardObject = Instantiate(_programCardPrefab, transform);
        cardObject.HighlightParent = _highlightParent;
        cardObject.enabled = false;

        cardObject.SetContent(card);
        _cardObjects.Insert(index, cardObject);

        UpdateCards();
        return cardObject;
    }

    void CreateHand() {
        for (var i = 0; i < Owner.Hand.Cards.Count; i++) {
            var card = Owner.Hand.Cards[i];
            _actionQueue.Add(new CardAction {
                Type = CardActionType.Draw,
                Card = card,
                Index = i,
            });
        }
    }

    void UpdateCards() {
        for (var i = 0; i < _cardObjects.Count; i++) {
            var (origin, offset) = GetOriginAndOffset(i);
            _cardObjects[i].SetOrigin(origin, i, offset);
        }
    }

    (Vector2 Origin, float VerticalOffset) GetOriginAndOffset(int index) {
        var centeredIndex = index - _cardObjects.Count / 2f + 0.5f;
        var xPos = CardSpacing * centeredIndex;
        var yPos = -Mathf.Abs(centeredIndex) * _cardVerticalOffset;
        var pos = (Vector2) transform.position + CanvasUtils.CanvasScale * new Vector2(xPos, yPos);
        return (pos, yPos);
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