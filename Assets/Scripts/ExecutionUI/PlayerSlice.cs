using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerSlice : Container<Player>, IPointerClickHandler {
    [Header("Player Slice")]
    [SerializeField] OverlayData<PlayerOverlay> _overlayData;

    [Header("References")]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;
    [SerializeField] ExecutionRegister[] _executionRegisters;
    
    Player _player;

    protected override void Serialize(Player player) {
        if (_player != null) {
            _player.Energy.OnValueChanged -= OnEnergyChanged;
        }
        
        _player = player;
        
        _nameText.text = PlayerManager.IsLocal(_player) ? _player + " (You)" : _player.ToString();
        player.Energy.OnValueChanged += OnEnergyChanged;
    }

    void Awake() {
        ExecutionPhase.OnPhaseStart += OnPhaseStart;
    }

    void OnDestroy() {
        if (_player != null) {
            _player.Energy.OnValueChanged -= OnEnergyChanged;
        }
        ExecutionPhase.OnPhaseStart -= OnPhaseStart;
    }

    void OnEnergyChanged(int prev, int next) {
        _energyText.text = next.ToString();
    }
    
    void OnPhaseStart() {
        var local = PlayerManager.IsLocal(_player);
        for (var i = 0; i < _executionRegisters.Length; i++){
            var register = _executionRegisters[i];
            register.SetContent(_player.Program[i]);
            register.Hidden = !local;
        }
    }
    
    public void Show(int index) {
        _executionRegisters[index].Hidden = false;
    }
    
    public void OnPointerClick(PointerEventData e) {
        OverlaySystem.Instance.ShowOverlay(_overlayData).Init(_player);
    }
}