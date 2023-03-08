using System.Text;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class PushPanel : BoardElement<PushPanel, ICanEnterHandler>, ITooltipable, ITriggerAwake {
    [Header("Push Panel")]
    [SerializeField] int[] _activeRegisters; 
    [SerializeField] Vector2Int _direction;

    [Header("Animation")]
    [SerializeField] float _pushSpeed;
    [SerializeField] LeanTweenType _pushEaseType;
    [SerializeField] PassiveAnimation _animation;
    
    [Header("References")]
    [SerializeField] TMP_Text[] _registerTexts;

    public string Header => "Push Panel";
    public string Description {
        get {
            var registers = _activeRegisters.Select(r => (r + 1).ToString()).ToArray();
            var str = new StringBuilder("Pushes objects one tile on ");
            str.Append(registers.Length == 1 ? "register " : "registers ");
            str.Append(registers[0]);
            
            for (var i = 1; i < registers.Length; i++) {
                str.Append($", {registers[i]}");
            }
            str.Append(".");
            return str.ToString();
        }
    }

    public void TriggerAwake() => Awake();
    protected override void Awake() {
        base.Awake();
        _direction = Rotator.Rotate(_direction);
        OnRotationChanged += s => {
            _direction = _direction.Transform(s);
        };

        for (var i = 0; i < _activeRegisters.Length; i++) {
            _registerTexts[i].text = (_activeRegisters[i] + 1).ToString();
        }
    }

    protected override void Activate(ICanEnterHandler[] targets) {
        var pushable = targets.Where(t => t.Movable).ToArray();
        if (pushable.Length == 0) return;
        
        if (!_activeRegisters.Contains(ExecutionPhase.CurrentRegister) ||
            !Interaction.Push(pushable[0].Object, _direction, out var action)) return;
        
        AddActivation();
        
        action.MapObjects.AddRange(pushable.Skip(1).Select(o => o.Object));
        TaskScheduler.PushRoutine(Routine());

        IEnumerator Routine() {
            _animation.PlayOnce();
            yield return Interaction.EaseEvent(action, _pushEaseType, _pushSpeed);
        }
    }
}