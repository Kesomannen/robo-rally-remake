using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExecutionUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] PlayerExecutionPanels _panelsController;
    [SerializeField] ProgramCardViewer _programCardViewer;
    
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
        _phaseIcon2.transform.position = pos + Vector3.up * _phaseDistance * CanvasUtils.CanvasScale.x;
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
    }

    void Awake() {
        ExecutionPhase.OnPlayerRegistersComplete += OnPlayerRegistersComplete;
        ExecutionPhase.OnNewSubPhase += OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister += OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered += OnPlayersOrdered;
    }

    void OnDestroy() {
        ExecutionPhase.OnPlayerRegistersComplete -= OnPlayerRegistersComplete;
        ExecutionPhase.OnNewSubPhase -= OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister -= OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered -= OnPlayersOrdered;
    }

    void OnPlayersOrdered(IReadOnlyList<Player> nextPlayerOrder) {
        var swaps = new List<(int first, int second)>();
        var order = _currentPlayerOrder.Copy();

        // Move rebooted players to the end
        if (PlayerSystem.Players.Any(p => p.IsRebooted.Value)) {
            var rebootIndex = order.Length - 1;
            for (var i = 0; i < order.Length; i++) {
                var player = order[i];
                if (!player.IsRebooted.Value) continue;
                swaps.Add((i, rebootIndex));
                (order[i], order[rebootIndex]) = (order[rebootIndex], order[i]);
                rebootIndex--;
            }
        }
        _currentPlayerOrder = nextPlayerOrder.ToArray();

        for (var i = 0; i < nextPlayerOrder.Count; i++) {
            var player = nextPlayerOrder[i];
            var current = order.IndexOf(player);
            if (i == current) continue;
            
            swaps.Add((i, current));
            (order[i], order[current]) = (order[current], order[i]);
        }

        StartCoroutine(swaps.Select(swap => _panelsController.Swap(swap.first, swap.second)).GetEnumerator());
    
        ChangeSubPhase(UISubPhase.OrderPlayers);
    }

    static void BalanceScale(PlayerExecutionPanel panel, PlayerExecutionRegister target, float scale) {
        var registerCount = panel.Registers.Count;
        var balancedScale = (registerCount - scale) / (registerCount - 1);
        foreach (var register in panel.Registers) {
            register.Scale = register == target ? scale : balancedScale;
        }
    }
    
    void OnPlayerRegister(ProgramCardData card, int index, Player player) {
        player.Model.Highlight(true);
        
        var playerIndex = _currentPlayerOrder.IndexOf(player);
        var panel = _panelsController.Panels[playerIndex];
        var register = panel.Registers[index];
        
        register.Visible = true;
        BalanceScale(panel, register, register.Scale + 0.2f);
        
        if (playerIndex <= 0) return;
        
        // Unhighlight previous player
        var prevPlayer = _currentPlayerOrder[playerIndex - 1];
        prevPlayer.Model.Highlight(false);
        
        var prevPanel = _panelsController.Panels[playerIndex - 1];
        var prevRegister = prevPanel.Registers[index];
        BalanceScale(prevPanel, prevRegister, prevRegister.Scale - 0.2f);
    }

    void OnPlayerRegistersComplete() {
        StartCoroutine(_programCardViewer.ClearCards());
        foreach (var playerPanel in _panelsController.Panels) {
            BalanceScale(playerPanel, playerPanel.Registers[ExecutionPhase.CurrentRegister], 1);
            playerPanel.Content.Model.Highlight(false);
        }
    }

    void OnNewSubPhase(ExecutionSubPhase executionSubPhase) {
        if ((int)executionSubPhase == 0) {
            foreach (var playerPanel in _panelsController.Panels) {
                BalanceScale(playerPanel, playerPanel.Registers[ExecutionPhase.CurrentRegister], 1.05f);
            }
        }

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