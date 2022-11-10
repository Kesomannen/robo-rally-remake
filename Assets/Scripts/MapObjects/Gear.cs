using UnityEngine;

public class Gear : BoardElement<Gear> {
    [SerializeField] int _rotationSteps;

    protected override void OnActivate() {
        Scheduler.Push(CurrentDynamic.RotateRoutine(_rotationSteps));
    }
}