using System.Collections;
using UnityEngine;

public class Gear : BoardElement<Gear, IMapObject> {
    [SerializeField] int _rotationSteps;

    protected override void Activate(IMapObject[] targets) {
        var routines = new IEnumerator[targets.Length];
        for (int i = 0; i < targets.Length; i++) {
            var target = targets[i].Object;
            routines[i] = target.RotateRoutine(_rotationSteps);
        }
        Scheduler.Push(Scheduler.GroupRoutines(routines), $"Gear {_rotationSteps} steps");
    }
}