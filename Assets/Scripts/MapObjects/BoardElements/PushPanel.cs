using System.Text;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class PushPanel : BoardElement<PushPanel, IObstacle>, ITooltipable {
    [SerializeField] int[] _activeRegisters; 
    [SerializeField] Vector2Int _direction;
    [SerializeField] TMP_Text[] _registerTexts;

    public string Header => "Push Panel";
    public string Description {
        get {
            var str = new StringBuilder("Pushes objects one space in the direction of the panel.\nActivates on register {_activeRegisters[0]}");
            for (int i = 1; i < _activeRegisters.Length; i++) {
                str.Append($", {_activeRegisters[i]}");
            }
            str.Append(".");
            return str.ToString();
        }
    }

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

    protected override void Activate(IObstacle[] targets) {
        if (_activeRegisters.Contains(ExecutionPhase.CurrentRegister)) {
            var pushRoutines = new IEnumerator[targets.Length];
            for (int i = 0; i < targets.Length; i++) {
                var target = targets[i];
                pushRoutines[i] = MapHelper.PushRoutine(target, _direction);
            }
            Scheduler.Push(Scheduler.GroupRoutines(pushRoutines), "PushPanel");
        }
    }
}