using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class BoardElement<T, THandler> : MapObject, IOnEnterExitHandler where THandler : IMapObject where T : BoardElement<T, THandler> {
    protected List<THandler> Handlers;

    protected static Action OnActivateEvent;

    protected abstract void Activate(THandler[] targets);

    public static IEnumerator ActivateElement() {
        OnActivateEvent?.Invoke();
        yield return Scheduler.WaitUntilClearRoutine();
    }
    
    public virtual void OnEnter(MapObject mapObject) {
        if (mapObject is not THandler handler) return;
        Handlers ??= new List<THandler>();
        Handlers.Add(handler);
        if (Handlers.Count == 1) OnActivateEvent += OnActivate;
    }

    public virtual void OnExit(MapObject mapObject) {
        if (mapObject is THandler handler) {
            Handlers.Remove(handler);
            if (Handlers.Count == 0) OnActivateEvent -= OnActivate;
        }
    }

    void OnActivate() => Activate(Handlers.ToArray());
}