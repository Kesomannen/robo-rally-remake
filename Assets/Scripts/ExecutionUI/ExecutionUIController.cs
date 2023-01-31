using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExecutionUIController : MonoBehaviour {
    [Header("Player Panels")]
    [SerializeField] PlayerExecutionPanels _panelsController;
    
    [Header("SubPhase")]
    [SerializeField] TMP_Text _subPhaseText;
    [SerializeField] Image _phaseIcon1;
    [SerializeField] Image _phaseIcon2;
    [SerializeField] float _phaseMoveTime;
    [SerializeField] float _phaseDistance;
    [SerializeField] LeanTweenType _phaseTweenType;

    Vector3 _iconPosition;
    Image _currentSubPhaseImage;
    
    [SerializeField] SubPhaseInfo 
        _orderPlayers,
        _playerRegisters,
        _conveyor,
        _pushPanel,
        _gear,
        _boardLaser,
        _playerLaser,
        _energySpace,
        _checkpoint;

    enum UISubPhase {
        OrderPlayers,
        PlayerRegisters,
        Conveyor,
        PushPanel,
        Gear,
        BoardLaser,
        PlayerLaser,
        EnergySpace,
        Checkpoint
    }

    static UISubPhase Remap(ExecutionSubPhase x) {
        return (UISubPhase)((int)x + 1);
    }

    SubPhaseInfo GetInfo(UISubPhase uiSubPhase) {
        return uiSubPhase switch {
            UISubPhase.OrderPlayers => _orderPlayers,
            UISubPhase.PlayerRegisters => _playerRegisters,
            UISubPhase.Conveyor => _conveyor,
            UISubPhase.PushPanel => _pushPanel,
            UISubPhase.Gear => _gear,
            UISubPhase.BoardLaser => _boardLaser,
            UISubPhase.PlayerLaser => _playerLaser,
            UISubPhase.EnergySpace => _energySpace,
            UISubPhase.Checkpoint => _checkpoint,
            _ => throw new ArgumentOutOfRangeException(nameof(uiSubPhase), uiSubPhase, null)
        };
    }

    UISubPhase _currentSubPhase = UISubPhase.OrderPlayers;

    void Start() {
        var pos = _phaseIcon1.transform.position;
        _iconPosition = pos;
        _phaseIcon2.transform.position = pos + Vector3.up * _phaseDistance;
        _currentSubPhaseImage = _phaseIcon2;

        foreach (var player in PlayerSystem.Players) {
            _panelsController.CreatePanel(player);
        }
        
        gameObject.SetActive(false);
    }

    void ChangeSubPhase(UISubPhase uiSubPhase) {
        if (_currentSubPhase == uiSubPhase) return;
        _currentSubPhase = uiSubPhase;
        
        var info = GetInfo(uiSubPhase);

        var current = _currentSubPhaseImage;
        var next = current == _phaseIcon1 ? _phaseIcon2 : _phaseIcon1;
        
        next.sprite = uiSubPhase switch {
            UISubPhase.PlayerRegisters => info.Icons[ExecutionPhase.CurrentRegister],
            UISubPhase.EnergySpace => ExecutionPhase.CurrentRegister == 4 ? info.Icons[1] : info.Icons[0],
            _ => info.Icons[0]
        };

        StartCoroutine(Animation());
        
        IEnumerator Animation() {
            LeanTween
                .move(current.gameObject, _iconPosition - Vector3.up * _phaseDistance, _phaseMoveTime)
                .setEase(_phaseTweenType);
            
            LeanTween
                .move(next.gameObject, _iconPosition, _phaseMoveTime)
                .setEase(_phaseTweenType);
            
            yield return CoroutineUtils.Wait(_phaseMoveTime / 2);
            _subPhaseText.text = info.Name;
            yield return CoroutineUtils.Wait(_phaseMoveTime / 2);

            current.transform.position = _iconPosition + Vector3.up * _phaseDistance;
            _currentSubPhaseImage = next;
        }
    }

    void Awake() {
        _phaseDistance *= CanvasUtils.CanvasScale.x;
        
        ExecutionPhase.OnNewRegister += OnNewRegister;
        ExecutionPhase.OnNewSubPhase += OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister += OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered += OnPlayersOrdered;
    }

    void OnDestroy() {
        ExecutionPhase.OnNewRegister -= OnNewRegister;
        ExecutionPhase.OnNewSubPhase -= OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister -= OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered -= OnPlayersOrdered;
    }

    void OnPlayersOrdered(IReadOnlyList<Player> orderedPlayers) {
        ChangeSubPhase(UISubPhase.OrderPlayers);
    }
    
    void OnPlayerRegister(ProgramCardData card, int index, Player player) {
        
    }

    void OnNewRegister(int register) {
        
    }

    void OnNewSubPhase(ExecutionSubPhase executionSubPhase) {
        ChangeSubPhase(Remap(executionSubPhase));
    }

    [Serializable]
    struct SubPhaseInfo {
        [SerializeField] string _name;
        [SerializeField] Sprite[] _icons;
        
        public string Name => _name;
        public Sprite[] Icons => _icons;
    }
}