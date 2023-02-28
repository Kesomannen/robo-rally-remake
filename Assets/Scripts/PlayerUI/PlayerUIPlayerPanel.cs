using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerUIPlayerPanel : PlayerPanel {
    [Header("Upgrade Card")]
    [SerializeField] UpgradeCard _upgradeCardPrefab;
    [FormerlySerializedAs("_upgradeCardDuration")] 
    [SerializeField] float _upgradeTweenDuration;
    [SerializeField] float _upgradeCardTime;
    [SerializeField] LeanTweenType _upgradeCardTweenType;
    [SerializeField] Transform _upgradeCardStart;
    [SerializeField] Transform _upgradeCardEnd;
    
    [Header("Other Indicators")]
    [SerializeField] GameObject _energyPrefab;
    [SerializeField] GameObject _programCardPrefab;
    
    [SerializeField] float _indicatorDuration;
    [SerializeField] float _indicatorInterval;
    [SerializeField] LeanTweenType _indicatorTweenType;
    [SerializeField] float _indicatorScale;

    bool _programLockedIn;

    protected override void Serialize(Player player) {
        base.Serialize(player);
        
        Content.OnUpgradeUsed += OnUpgradeUsed;
        Content.Energy.OnValueChanged += OnEnergyChanged;
        Content.OnCardDealt += OnCardDealt;

        ProgrammingPhase.OnPhaseStarted += OnPhaseStarted;
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerProgramDone;

        OnPhaseStarted();
    }
    
    void OnCardDealt(ProgramCardData card, Pile pile) {
        if (!enabled) return;
        if (PhaseSystem.Current.Value != Phase.Programming) return;
        
        Debug.Log(_programCardPrefab, this);
        var obj = Instantiate(_programCardPrefab, _upgradeCardStart);
        obj.GetComponent<Container<ProgramCardData>>().SetContent(card);
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
        
        TaskScheduler.PushRoutine(DoIndicatorAnimation(objects), delay: 0);
    }

    void OnUpgradeUsed(UpgradeCardData upgrade) {
        if (!enabled) return;

        var obj = Instantiate(_upgradeCardPrefab, _upgradeCardStart);
        obj.SetContent(upgrade);
        var t = obj.transform;
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.zero;
        
        TaskScheduler.PushRoutine(Animation());
        
        IEnumerator Animation() {
            LeanTween.moveLocalX(obj.gameObject, _upgradeCardEnd.localPosition.x, 2 * _upgradeTweenDuration / 3).setEase(_upgradeCardTweenType);
            LeanTween.scale(obj.gameObject, Vector3.one, 2 * _upgradeTweenDuration / 3).setEase(_upgradeCardTweenType);
            
            yield return CoroutineUtils.Wait(2 * _upgradeTweenDuration / 3 + _upgradeCardTime);
            
            LeanTween.scale(obj.gameObject, Vector3.zero, _upgradeTweenDuration / 3).setEase(_upgradeCardTweenType);
            yield return CoroutineUtils.Wait(_upgradeTweenDuration / 3);
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