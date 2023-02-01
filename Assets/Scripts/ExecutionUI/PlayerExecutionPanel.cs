using System;
using TMPro;
using UnityEngine;

public class PlayerExecutionPanel : Container<Player> {
    [SerializeField] PlayerExecutionRegister[] _registers;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;

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
    }
    
    void OnEnergyChanged(int prev, int next) {
        _energyText.text = next.ToString();
    }
}