using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerExecutionPanel : Container<Player> {
    [Header("References")")]
    [SerializeField] PlayerExecutionRegister[] _registers;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;
    
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

        player.Energy.OnValueChanged += OnEnergyChanged;
        player.OnDamaged += OnDamaged;
    }
    
    void OnDamaged(CardAffector affector) {
        
    }

    void OnEnergyChanged(int prev, int next) {
        _energyText.text = next.ToString();
    }

    IEnumerator DoAnimation(IReadOnlyList<Transform> objects) {
        foreach (var obj in objects) {
            obj.position = _tweenStart.position;
            obj.localScale = Vector3.one;
            obj.SetParent(transform, true);
        }
        
        for (var i = 0; i < objects.Count; i++) {
            LeanTween
                .sequence()
                .append(_spawnDelay * i)
                .append(LeanTween.moveLocalX(objects[i].gameObject, objects[i].localPosition.x - _tweenDistance, _tweenTime).setEase(_tweenType))
                .append(LeanTween.moveLocalX(objects[i].gameObject, objects[i].localPosition.x, _tweenTime).setEase(_tweenType));
        }
    }
}