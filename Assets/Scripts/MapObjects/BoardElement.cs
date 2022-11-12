using System.Collections;
using System;

public abstract class BoardElement<T> : StaticObject where T : BoardElement<T> {
    protected DynamicObject CurrentDynamic { get; private set; }

    protected static Action _onActivate;

    public static IEnumerator ActivateRoutine() {
        _onActivate?.Invoke();
        yield return Scheduler.WaitUntilClearRoutine();
    }

    public override void OnEnter(DynamicObject dynamic) {
        CurrentDynamic = dynamic;
        _onActivate += OnActivate;
    }

    public override void OnExit(DynamicObject dynamic) {
        CurrentDynamic = null;
        _onActivate -= OnActivate;
    }

    void OnActivate() => Activate(CurrentDynamic);

    protected abstract void Activate(DynamicObject dynamic);
}