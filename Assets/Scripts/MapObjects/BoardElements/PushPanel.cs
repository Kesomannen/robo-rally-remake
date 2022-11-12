using System.Linq;
using TMPro;
using UnityEngine;

public class PushPanel : BoardElement<PushPanel> {
    [SerializeField] int[] _activeRegisters; 
    [SerializeField] Vector2Int _direction;
    [SerializeField] TMP_Text[] _registerTexts;

    protected override void Awake() {
        base.Awake();
        RotateAsObject(_direction);
        for (int i = 0; i < _registerTexts.Length; i++) {
            var text = _registerTexts[i];
            text.text = _activeRegisters[i].ToString();
        }
    }

    protected override void Activate(DynamicObject dynamic) {
        if (_activeRegisters.Contains(ExecutionPhase.CurrentRegister)) {
            Scheduler.Push(Interaction.PushRoutine(dynamic, _direction));
        }
    }
}