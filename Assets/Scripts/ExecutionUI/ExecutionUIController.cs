using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExecutionUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] TMP_Text _subPhaseText;
    [SerializeField] Image _subPhaseImage;
    
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
        gameObject.SetActive(false);
    }

    void SetSubPhaseUI(UISubPhase uiSubPhase) {
        if (_currentSubPhase == uiSubPhase) return;
        _currentSubPhase = uiSubPhase;
        
        var info = GetInfo(uiSubPhase);
        _subPhaseText.text = info.Name;
        _subPhaseImage.sprite = uiSubPhase switch {
            UISubPhase.PlayerRegisters => info.Icons[ExecutionPhase.CurrentRegister],
            UISubPhase.EnergySpace => ExecutionPhase.CurrentRegister == 4 ? info.Icons[1] : info.Icons[0],
            _ => info.Icons[0]
        };
    }

    void Awake() {
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
        SetSubPhaseUI(UISubPhase.OrderPlayers);
    }
    
    void OnPlayerRegister(ProgramCardData card, int index, Player player) {
        
    }

    void OnNewRegister(int register) {
        
    }

    void OnNewSubPhase(ExecutionSubPhase executionSubPhase) {
        SetSubPhaseUI(Remap(executionSubPhase));
    }

    [Serializable]
    struct SubPhaseInfo {
        [SerializeField] string _name;
        [SerializeField] Sprite[] _icons;
        
        public string Name => _name;
        public Sprite[] Icons => _icons;
    }
}