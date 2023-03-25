using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgramCardViewer : MonoBehaviour {
    [Header("References")]
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] UpgradeCard _upgradeCardPrefab;

    [Header("Layout")]
    [SerializeField] Vector2 _cardSize;
    [SerializeField] float _cardSpacing;
    
    [Header("Tween")]
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _tweenTime;
    
    Vector3 SpacedSize => (_cardSize + Vector2.one * _cardSpacing) * CanvasUtils.CanvasScale;

    readonly List<Transform> _cards = new();
    Player _lastPlayer;
    bool _isAnimating;

    void OnEnable() {
        ExecutionPhase.PlayerRegister += OnRegister;
        PlayerSystem.PlayerCreated += OnPlayerCreated;
    }

    void OnDisable() {
        ExecutionPhase.PlayerRegister -= OnRegister;
        PlayerSystem.PlayerCreated -= OnPlayerCreated;
    }
    
    void OnPlayerCreated(Player player) {
        player.ProgramCardExecuted += OnProgramCardPlayed;
        player.UpgradeUsed += upgrade => OnUpgradeUsed(player, upgrade);
    }
    
    void OnRegister(ProgramExecution execution) {
        StartCoroutine(ClearCards());
    }

    void OnUpgradeUsed(Player player, UpgradeCardData upgrade) {
        if (!enabled) return;
        
        StartCoroutine(AddCard(_upgradeCardPrefab, upgrade, true));
        _lastPlayer = player;
    }
    
    void OnProgramCardPlayed(ProgramExecution execution) {
        StartCoroutine(AddCard(_programCardPrefab, execution.CurrentCard, _lastPlayer != execution.Player));
        _lastPlayer = execution.Player;
    }

    IEnumerator AddCard<T>(Container<T> prefab, T content, bool replace) {
        yield return new WaitUntil(() => !_isAnimating);
        if (replace) yield return ClearCards();
        yield return DoAnimation(Instantiate(prefab, transform).SetContent(content).transform);
    }
    
    IEnumerator DoAnimation(Transform cardTransform) {
        _isAnimating = true;
        
        var t = transform;
        var targetPos = t.position + _cards.Count * Vector3.right;
        var startPos = _cards.Count > 0 ? 
            targetPos + SpacedSize.y * Vector3.down 
            : targetPos + SpacedSize.x * Vector3.left;
        
        cardTransform.position = startPos;
        _cards.Add(cardTransform);
        
        LeanTween.move(cardTransform.gameObject, targetPos, _tweenTime).setEase(_tweenType);
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