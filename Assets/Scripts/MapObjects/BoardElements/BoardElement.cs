using System;
using System.Collections.Generic;

public abstract class BoardElement<T, THandler> : MapObject, IOnEnterExitHandler where THandler : IMapObject where T : BoardElement<T, THandler> {
    List<THandler> _handlers;
    protected static int ActiveElements;
    
    protected static Action OnActivateEvent;

    protected abstract void Activate(THandler[] targets);

    public static bool ActivateElement() {
        if (ActiveElements == 0) return false;
        OnActivateEvent?.Invoke();
        return true;
    }
    
    public virtual void OnEnter(MapObject mapObject) {
        if (mapObject is not THandler handler) return;
        ActiveElements++;
        _handlers ??= new List<THandler>();
        _handlers.Add(handler);
        if (_handlers.Count == 1) OnActivateEvent += OnActivate;
    }

    public virtual void OnExit(MapObject mapObject) {
        if (mapObject is not THandler handler) return;
        ActiveElements--;
        _handlers?.Remove(handler);
        if (_handlers is { Count: 0 }) OnActivateEvent -= OnActivate;
    }

    void OnActivate() => Activate(_handlers.ToArray());
}