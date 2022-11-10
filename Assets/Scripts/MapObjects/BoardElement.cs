using System.Collections;
using System;

public abstract class BoardElement<T> : StaticObject where T : BoardElement<T> {
    protected static Action _onActivate;

    public static IEnumerator ActivateRoutine() {
        _onActivate?.Invoke();
        yield return Scheduler.WaitUntilClearRoutine();
    }

    public override void OnEnter(DynamicObject dynamic) {
        base.OnEnter(dynamic);
        _onActivate += OnActivate;
    }

    public override void OnExit(DynamicObject dynamic) {
        base.OnExit(dynamic);
        _onActivate -= OnActivate;
    }

    protected abstract void OnActivate();
}