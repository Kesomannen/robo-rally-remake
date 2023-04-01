using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ExecutionUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] PlayerExecutionPanels _panelsController;
    [SerializeField] ProgramCardViewer _programCardViewer;
    
    [Header("Colors")]
    [SerializeField] Color _unHighlightedColor;
    [SerializeField] Color _highlightedColor;
    [SerializeField] Color _activeColor;
    
    [Header("SubPhase")]
    [SerializeField] TMP_Text _subPhaseText;
    [SerializeField] Image _phaseIcon1;
    [SerializeField] Image _phaseIcon2;
    [SerializeField] float _phaseMoveTime;
    [SerializeField] float _phaseDistance;
    [SerializeField] LeanTweenType _phaseTweenType;

    Vector3 _iconPosition;
    Image _currentSubPhaseImage;
    List<Player> _playerPanelOrder;
    Player _previousPlayer;
    
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

    void Awake() {
        ExecutionPhase.PlayerRegistersComplete += OnPlayerRegistersComplete;
        ExecutionPhase.NewSubPhase += OnNewSubPhase;
        ExecutionPhase.PlayerRegister += OnPlayerRegister;
        ExecutionPhase.PlayersOrdered += OnPlayersOrdered;

        var pos = _phaseIcon1.transform.position;
        
        _iconPosition = pos;
        _phaseIcon2.transform.position = pos + Vector3.up * _phaseDistance * CanvasUtils.CanvasScale.y;
        _currentSubPhaseImage = _phaseIcon1;
        
        _playerPanelOrder = new List<Player>();
        foreach (var player in PlayerSystem.Instance.GetOrderedPlayers()) {
            _panelsController.CreatePanel(player);
            _playerPanelOrder.Add(player);
        }
    }
    
    void OnDestroy() {
        ExecutionPhase.PlayerRegistersComplete -= OnPlayerRegistersComplete;
        ExecutionPhase.NewSubPhase -= OnNewSubPhase;
        ExecutionPhase.PlayerRegister -= OnPlayerRegister;
        ExecutionPhase.PlayersOrdered -= OnPlayersOrdered;
    }
    
    IEnumerator ChangeSubPhase(UISubPhase uiSubPhase) {
        var info = GetInfo(uiSubPhase);
        var distance = CanvasUtils.CanvasScale.x * _phaseDistance;

        var current = _currentSubPhaseImage;
        var next = current == _phaseIcon1 ? _phaseIcon2 : _phaseIcon1;
            
        next.sprite = uiSubPhase switch {
            UISubPhase.PlayerRegisters => info.Icons[ExecutionPhase.CurrentRegister],
            UISubPhase.EnergySpace => ExecutionPhase.CurrentRegister == 4 ? info.Icons[1] : info.Icons[0],
            _ => info.Icons[0]
        };
            
        LeanTween
            .move(current.gameObject, _iconPosition - Vector3.up * distance, _phaseMoveTime)
            .setEase(_phaseTweenType);
            
        LeanTween
            .move(next.gameObject, _iconPosition, _phaseMoveTime)
            .setEase(_phaseTweenType);
            
        yield return CoroutineUtils.Wait(_phaseMoveTime / 2);
        _subPhaseText.text = info.Name;
        yield return CoroutineUtils.Wait(_phaseMoveTime / 2);

        current.transform.position = _iconPosition + Vector3.up * distance;
        _currentSubPhaseImage = next;
    }
    
    void OnPlayersOrdered(IReadOnlyList<Player> nextPlayerOrder) {
        var swaps = new List<(int first, int second)>();

        for (var i = 0; i < nextPlayerOrder.Count; i++) {
            var player = nextPlayerOrder[i];
            var current = _playerPanelOrder.IndexOf(player);
            if (i == current) continue;
            
            swaps.Add((i, current));
            (_playerPanelOrder[i], _playerPanelOrder[current]) = (_playerPanelOrder[current], _playerPanelOrder[i]);
        }
        
        TaskScheduler.PushSequence(routines: swaps.Select(swap => DoSwap(swap.first, swap.second)).ToArray());
        TaskScheduler.PushRoutine(ChangeSubPhase(UISubPhase.OrderPlayers));

        IEnumerator DoSwap(int first, int second) {
            yield return Antenna.Instance.BeamAnimation(nextPlayerOrder[first]);
            yield return _panelsController.Swap(first, second);
        }
    }

    static void SetColor(PlayerExecutionPanel panel, Color color) {
        foreach (var register in panel.Registers) {
            register.Color = color;
        }
    }
    
    static void BalanceScale(PlayerExecutionPanel panel, int index, float scale) {
        BalanceScale(panel, panel.Registers[index], scale);
    }
    
    static void BalanceScale(PlayerExecutionPanel panel, PlayerExecutionRegister target, float scale) {
        var registerCount = panel.Registers.Count;
        var balancedScale = (registerCount - scale) / (registerCount - 1);
        foreach (var register in panel.Registers) {
            register.Scale = register == target ? scale : balancedScale;
        }
    }

    void OnPlayerRegister(ProgramExecution execution) {
        var player = execution.Player;
        
        player.Model.Highlight(true);
        
        var panel = _panelsController.Panels.First(panel => panel.Content == player);
        var register = panel.Registers[execution.Register];
        
        register.Visible = true;
        register.Color = _activeColor;
        BalanceScale(panel, register, register.Scale + 0.15f);

        if (_previousPlayer != null) {
            // Unhighlight previous player
            _previousPlayer.Model.Highlight(false);
            
            var prevPanel = _panelsController.Panels.First(prevPanel => prevPanel.Content == _previousPlayer);
            var prevRegister = prevPanel.Registers[execution.Register];
            
            prevRegister.Color = _highlightedColor;
            BalanceScale(prevPanel, prevRegister, prevRegister.Scale - 0.15f);
        }
        _previousPlayer = player;
    }

    void OnPlayerRegistersComplete() {
        StartCoroutine(_programCardViewer.ClearCards());
        foreach (var playerPanel in _panelsController.Panels) {
            BalanceScale(playerPanel, ExecutionPhase.CurrentRegister, 1);
            SetColor(playerPanel, _unHighlightedColor);
            playerPanel.Content.Model.Highlight(false);
        }
        _previousPlayer = null;
    }

    void OnNewSubPhase(ExecutionSubPhase executionSubPhase) {
        var register = ExecutionPhase.CurrentRegister;
        
        if ((int)executionSubPhase == 0) {
            foreach (var playerPanel in _panelsController.Panels) {
                BalanceScale(playerPanel, register, 1.05f);
                playerPanel.Registers[register].Color = _highlightedColor;
            }
        }

        TaskScheduler.PushRoutine(ChangeSubPhase(Remap(executionSubPhase)));
    }

    [Serializable]
    struct SubPhaseInfo {
        [SerializeField] string _name;
        [SerializeField] Sprite[] _icons;
        
        public string Name => _name;
        public Sprite[] Icons => _icons;
    }
}