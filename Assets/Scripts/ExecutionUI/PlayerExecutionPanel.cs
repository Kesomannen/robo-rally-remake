using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerExecutionPanel : Container<Player> {
    [Header("References")]
    [SerializeField] PlayerExecutionRegister[] _registers;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;
    [SerializeField] GameObject _rebootedOverlay;
    
    [Header("Prefabs")]
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] GameObject _energyPrefab;
    
    [Header("Tween")]
    [SerializeField] Transform _tweenStart;
    [SerializeField] float _tweenTime;
    [SerializeField] float _tweenDistance;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _spawnDelay;

    public IReadOnlyList<PlayerExecutionRegister> Registers => _registers;

    void Awake() {
        ExecutionPhase.OnPhaseStart += OnExecutionStart;
    }

    void OnDestroy() {
        ExecutionPhase.OnPhaseEnd -= OnExecutionStart;
    }

    void OnExecutionStart() {
        var isLocal = PlayerSystem.IsLocal(Content);
        for (var i = 0; i < _registers.Length; i++) {
            _registers[i].SetContent(Content.Program[i]);
            _registers[i].Visible = isLocal;
        }
    }

    protected override void Serialize(Player player) {
        _nameText.text = PlayerSystem.IsLocal(player) ? player + " (You)" : player.ToString();
        _energyText.text = player.Energy.ToString();
        _rebootedOverlay.SetActive(player.IsRebooted.Value);

        player.Energy.OnValueChanged += OnEnergyChanged;
        player.IsRebooted.OnValueChanged += OnIsRebootedChanged;
        player.OnDamaged += OnDamaged;
    }

    void OnIsRebootedChanged(bool prev, bool next) {
        _rebootedOverlay.SetActive(next);
    }
    
    void OnDamaged(CardAffector affector) {
        var objects = new Transform[affector.Cards.Count];
        for (var i = 0; i < affector.Cards.Count; i++) {
            var card = Instantiate(_programCardPrefab);
            card.SetContent(affector.Cards[i]);
            objects[i] = card.transform;
        }
        TaskScheduler.PushRoutine(DoAnimation(objects));
    }

    void OnEnergyChanged(int prev, int next) {
        if (next > prev) {
            var objects = new Transform[next - prev];
            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Instantiate(_energyPrefab).transform;
            }
            TaskScheduler.PushRoutine(DoAnimation(objects));
        }
        
        _energyText.text = next.ToString();
    }

    IEnumerator DoAnimation(IReadOnlyList<Transform> objects) {
        foreach (var obj in objects) {
            obj.SetParent(_tweenStart, true);
            obj.localPosition = Vector3.zero;
            obj.localScale = Vector3.zero;
        }

        var distance = _tweenDistance * CanvasUtils.CanvasScale.x;
        for (var i = 0; i < objects.Count; i++) {
            LeanTween
                .sequence()
                .append(_spawnDelay * i)
                .append(LeanTween.moveLocalX(objects[i].gameObject, -distance, _tweenTime).setEase(_tweenType))
                .append(LeanTween.moveLocalX(objects[i].gameObject, 0, _tweenTime).setEase(_tweenType));
            LeanTween
                .sequence()
                .append(_spawnDelay * i)
                .append(LeanTween.scale(objects[i].gameObject, Vector3.one, _tweenTime).setEase(_tweenType))
                .append(LeanTween.scale(objects[i].gameObject, Vector3.zero, _tweenTime).setEase(_tweenType));
        }
        yield return CoroutineUtils.Wait(_tweenTime * 2 + _spawnDelay * objects.Count);
        foreach (var obj in objects) {
            Destroy(obj.gameObject);
        }
    }
}