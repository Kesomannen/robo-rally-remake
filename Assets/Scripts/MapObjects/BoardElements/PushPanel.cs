using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class PushPanel : BoardElement<PushPanel, IPushable> {
    [SerializeField] int[] _activeRegisters; 
    [SerializeField] Vector2Int _direction;
    [SerializeField] TMP_Text[] _registerTexts;

    protected override void Awake() {
        base.Awake();
        _direction = Rotator.Rotate(_direction);
        OnRotationChanged += s => {
            _direction = _direction.Transform(s);
        };

        for (int i = 0; i < _registerTexts.Length; i++) {
            var text = _registerTexts[i];
            text.text = _activeRegisters[i].ToString();
        }
    }

    protected override void Activate(IPushable[] targets) {
        if (_activeRegisters.Contains(ExecutionPhase.CurrentRegister)) {
            var pushRoutines = new IEnumerator[targets.Length];
            for (int i = 0; i < targets.Length; i++) {
                var target = targets[i];
                pushRoutines[i] = Interaction.PushRoutine(target, _direction);
            }
            Scheduler.Push(Scheduler.GroupRoutines(pushRoutines), "PushPanel");
        }
    }
}