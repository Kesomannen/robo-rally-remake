using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [ReadOnly] [SerializeField] Image _currentSubPhaseImage;
    
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
        None,
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
        return (UISubPhase)((int)x + 2);
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

    UISubPhase _currentSubPhase = UISubPhase.None;
    Player[] _currentPlayerOrder;

    void Start() {
        var pos = _phaseIcon1.transform.position;
        _iconPosition = pos;
        _phaseIcon2.transform.position = pos + Vector3.up * _phaseDistance;
        _currentSubPhaseImage = _phaseIcon1;

        _currentPlayerOrder = PlayerSystem.Players.ToArray();
        foreach (var player in PlayerSystem.Players) {
            _panelsController.CreatePanel(player);
        }
        
        gameObject.SetActive(false);
    }

    void ChangeSubPhase(UISubPhase uiSubPhase) {
        if (_currentSubPhase == uiSubPhase) return;
        _currentSubPhase = uiSubPhase;

        TaskScheduler.PushRoutine(Animation());
        
        IEnumerator Animation() {
            var info = GetInfo(uiSubPhase);

            var current = _currentSubPhaseImage;
            var next = current == _phaseIcon1 ? _phaseIcon2 : _phaseIcon1;
            
            next.sprite = uiSubPhase switch {
                UISubPhase.PlayerRegisters => info.Icons[ExecutionPhase.CurrentRegister],
                UISubPhase.EnergySpace => ExecutionPhase.CurrentRegister == 4 ? info.Icons[1] : info.Icons[0],
                _ => info.Icons[0]
            };
            
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

    void OnPlayersOrdered(IReadOnlyList<Player> nextPlayerOrder) {
        var swaps = new List<(int first, int second)>();

        for (var current = 0; current < _currentPlayerOrder.Length; current++) {
            var player = _currentPlayerOrder[current];
            var target = nextPlayerOrder.IndexOf(player);
            if (current == target || swaps.Any(
                    swap => (swap.first == current && swap.second == target) 
                         || (swap.second == target && swap.second == current))
                ) continue;
            swaps.Add((current, target));
        }
        
        TaskScheduler.PushSequence(routines: swaps.Select(swap => _panelsController.Swap(swap.first, swap.second)).ToArray());

        _currentPlayerOrder = nextPlayerOrder.ToArray();
        
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