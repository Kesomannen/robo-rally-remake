using UnityEngine;

public class Gear : BoardElement<Gear> {
    [SerializeField] int _rotationSteps;

    protected override void Activate(DynamicObject dynamic) {
        Scheduler.Push(dynamic.RotateRoutine(_rotationSteps));
    }
}