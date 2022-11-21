using System.Collections;
using UnityEngine;

public class Gear : BoardElement<Gear, IMapObject>, ITooltipable {
    [SerializeField] int _rotationSteps;

    public string Header => "Gear";
    public string Description => $"Rotates objects {90 * _rotationSteps} degrees after each register.";
    
    protected override void Activate(IMapObject[] targets) {
        var routines = new IEnumerator[targets.Length];
        for (var i = 0; i < targets.Length; i++) {
            var target = targets[i].Object;
            routines[i] = target.RotateRoutine(_rotationSteps);
        }
        Scheduler.Push(Scheduler.GroupRoutines(routines), $"Gear {_rotationSteps} steps");
    }
}