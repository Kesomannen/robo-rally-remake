using TMPro;
using UnityEngine;

public class PlayerExecutionPanel : Container<Player> {
    [SerializeField] PlayerExecutionRegister[] _registers;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;

    Player _player;

    void Awake() {
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerLockedIn;
    }
    
    void OnPlayerLockedIn(Player player) {
        if (player != _player) return;
        for (var i = 0; i < _registers.Length; i++) {
            _registers[i].SetContent(_player.Program[i]);
        }
    }

    protected override void Serialize(Player player) {
        _player = player;
        var isLocal = PlayerSystem.IsLocal(player);
        
        _nameText.text = isLocal ? player + " (You)" : player.ToString();
        _energyText.text = player.Energy.ToString();
        
        foreach (var register in _registers) {
            register.Hidden = !isLocal;
        }

        player.Energy.OnValueChanged += OnEnergyChanged;
    }
    
    void OnEnergyChanged(int prev, int next) {
        _energyText.text = next.ToString();
    }
}