using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgramCardViewer : MonoBehaviour {
    [Header("References")]
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] TMP_Text _headerText;

    [Header("Layout")]
    [SerializeField] Vector2 _cardSize;
    [SerializeField] float _cardSpacing;
    
    [Header("Tween")]
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenTime;
    
    Vector3 SpacedSize => (_cardSize + Vector2.one * _cardSpacing) * CanvasUtils.CanvasScale;

    readonly List<ProgramCard> _cards = new();
    Player _lastPlayer;
    bool _isAnimating;
    
    void Start() {
        foreach (var player in PlayerSystem.Players) {
            player.OnProgramCardExecuted += execution => OnProgramCardPlayed(player, execution.Card);
        }
    }
    
    void OnProgramCardPlayed(Player player, ProgramCardData card) {
        StartCoroutine(AddCard(card, _lastPlayer != player));
        _lastPlayer = player;
    }

    IEnumerator AddCard(ProgramCardData data, bool replace = false) {
        yield return new WaitUntil(() => !_isAnimating);
        _isAnimating = true;
        
        if (replace) {
            yield return ClearCards();
        }
        
        var t = transform;
        
        var targetPos = t.position + _cards.Count * Vector3.right;
        var startPos = _cards.Count > 0 ? targetPos + SpacedSize.y * Vector3.down : targetPos + SpacedSize.x * Vector3.left;
        
        var newCard = Instantiate(_programCardPrefab, t);
        newCard.SetContent(data);
        newCard.transform.position = startPos;
        _cards.Add(newCard);
        
        LeanTween.move(newCard.gameObject, targetPos, _tweenTime).setEase(_tweenType);
        yield return CoroutineUtils.Wait(_tweenTime);
        _isAnimating = false;
    }

    public IEnumerator ClearCards() {
        _isAnimating = true;
        foreach (var card in _cards) {
            var pos = card.transform.position + SpacedSize.y * Vector3.down;
            LeanTween.move(card.gameObject, pos, _tweenTime).setEase(_tweenType);
            yield return CoroutineUtils.Wait(0.1f);
        }
        yield return CoroutineUtils.Wait(_tweenTime);
        foreach (var card in _cards) {
            Destroy(card.gameObject);
        }
        _cards.Clear();
        _isAnimating = false;
    }
}