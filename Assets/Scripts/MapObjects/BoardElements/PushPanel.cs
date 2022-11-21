using System.Text;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class PushPanel : BoardElement<PushPanel, ICanEnterHandler>, ITooltipable {
    [SerializeField] int[] _activeRegisters; 
    [SerializeField] Vector2Int _direction;
    [SerializeField] TMP_Text[] _registerTexts;

    public string Header => "Push Panel";
    public string Description {
        get {
            var registers = _activeRegisters.Select(r => (r + 1).ToString()).ToArray();
            var str = new StringBuilder($"Pushes objects one space in the direction of the panel. Activates on ");
            str.Append(registers.Length == 1 ? $"register" : $"registers");
            str.Append(registers[0]);
            
            for (var i = 1; i < registers.Length; i++) {
                str.Append($", {registers[i]}");
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

    protected override void Activate(ICanEnterHandler[] targets) {
        if (_activeRegisters.Contains(ExecutionPhase.CurrentRegister)) {
            if (Interaction.Push(targets[0].Object, _direction, out var action)) {
                action.MapObjects.AddRange(targets.Skip(1).Select(o => o.Object));
            }
            Scheduler.Push(Interaction.EaseEvent(action), "PushPanel");
        }
    }
}