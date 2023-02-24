using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIPlayerPanel : PlayerPanel {
    [Header("Upgrade Card")]
    [SerializeField] UpgradeCard _upgradeCardPrefab;
    [SerializeField] float _upgradeCardDuration;
    [SerializeField] LeanTweenType _upgradeCardTweenType;
    [SerializeField] Transform _upgradeCardStart;
    [SerializeField] Transform _upgradeCardEnd;
    
    [Header("Other Indicators")]
    [SerializeField] GameObject _energyPrefab;
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] float _indicatorDuration;
    [SerializeField] float _indicatorInterval;
    [SerializeField] LeanTweenType _indicatorTweenType;
    [SerializeField] float _indicatorScale;

    bool _programLockedIn;

    protected override void Serialize(Player player) {
        base.Serialize(player);
        
        Content.OnUpgradeUsed += OnUpgradeUsed;
        Content.Energy.OnValueChanged += OnEnergyChanged;
        Content.DrawPile.OnAdd += OnCardGet;
        Content.DiscardPile.OnAdd += OnCardGet;

        ProgrammingPhase.OnPhaseStarted += OnPhaseStarted;
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerProgramDone;

        OnPhaseStarted();
    }
    
    void OnCardGet(ProgramCardData card, int index) {
        if (!enabled) return;
        
        Debug.Log(_programCardPrefab);
        var obj = Instantiate(_programCardPrefab, _upgradeCardStart);
        obj.SetContent(card);
        var t = obj.transform;
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.zero;

        TaskScheduler.PushRoutine(DoIndicatorAnimation(new [] { t }));
    }

    IEnumerator DoIndicatorAnimation(IReadOnlyCollection<Transform> targets) {
        var totalDuration = (_indicatorDuration + _indicatorInterval) * (1 + Mathf.Log10(targets.Count));
        var perTargetDuration = totalDuration / targets.Count;
        
        foreach (var target in targets) {
            LeanTween
                .sequence()
                .append(LeanTween.scale(target.gameObject, Vector3.one * _indicatorScale, 2 * perTargetDuration / 3).setEase(_indicatorTweenType))
                .append(LeanTween.scale(target.gameObject, Vector3.zero, perTargetDuration / 3).setEase(_indicatorTweenType));

            yield return CoroutineUtils.Wait(_indicatorInterval);
        }
        yield return CoroutineUtils.Wait(perTargetDuration);
        foreach (var target in targets) {
            Destroy(target.gameObject);
        }
    }
    
    void OnEnergyChanged(int prev, int next) {
        if (!enabled) return;
        
        var delta = next - prev;
        if (delta < 0) return;
        
        var objects = new Transform[delta];
        for (var i = 0; i < delta; i++) {
            var obj = Instantiate(_energyPrefab, _upgradeCardStart);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.zero;
            objects[i] = obj.transform;
        }
        
        TaskScheduler.PushRoutine(DoIndicatorAnimation(objects));
    }

    void OnUpgradeUsed(UpgradeCardData upgrade) {
        if (!enabled) return;

        var obj = Instantiate(_upgradeCardPrefab, _upgradeCardStart);
        obj.SetContent(upgrade);
        var t = obj.transform;
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.zero;

        LeanTween.moveLocalX(obj.gameObject, _upgradeCardEnd.localPosition.x, 2 * _upgradeCardDuration / 3).setEase(_upgradeCardTweenType);
        LeanTween.scale(obj.gameObject, Vector3.one, 2 * _upgradeCardDuration / 3).setEase(_upgradeCardTweenType);
        TaskScheduler.PushRoutine(HideAnimation());
        
        IEnumerator HideAnimation() {
            LeanTween.scale(obj.gameObject, Vector3.zero, _upgradeCardDuration / 3).setEase(_upgradeCardTweenType);
            yield return CoroutineUtils.Wait(_upgradeCardDuration / 3);
            Destroy(obj.gameObject);
        }
    }

    void OnPlayerProgramDone(Player player) {
        if (Content == player) {
            _programLockedIn = true;
            SetIndicatorState(IndicatorState.Done);
        } else {
            SetIndicatorState(_programLockedIn ? IndicatorState.Done : IndicatorState.InProgress);
        }
    }

    void OnPhaseStarted() {
        _programLockedIn = false;
        SetIndicatorState(IndicatorState.Waiting);
    }
}