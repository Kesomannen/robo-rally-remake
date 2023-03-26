using System.Collections;
using System.Linq;
using UnityEngine;

public class Gear : BoardElement<Gear, IMapObject>, ITooltipable {
    [SerializeField] int _rotationSteps;

    public string Header => "Gear";
    public string Description {
        get {
            var direction = _rotationSteps < 0 ? "right" : "left";
            return $"Rotates objects to the {direction} after each register.";
        }
    }

    protected override void Activate(IMapObject[] targets) {
        var movable = targets.Where(t => t.Object.CanRotate && (t is not ICanEnterHandler handler || handler.Movable)).ToArray();
        var routines = new IEnumerator[movable.Length];
        for (var i = 0; i < movable.Length; i++) {
            AddActivation();
            
            routines[i] = movable[i].Object.RotateRoutine(_rotationSteps);
        }
        TaskScheduler.PushRoutine(this.RunRoutines(routines));
    }
}