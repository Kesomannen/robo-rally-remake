using System;
using System.Collections.Generic;

public abstract class BoardElement<T, THandler> : MapObject, IOnEnterExitHandler where THandler : IMapObject where T : BoardElement<T, THandler> {
    List<THandler> _handlers;
    protected static int ActiveElements;
    
    protected static Action ActivateEvent;

    protected abstract void Activate(THandler[] targets);

    static int _activations;
    protected void AddActivation() => _activations++;

    public static bool ActivateElement() {
        if (ActiveElements == 0) return false;
        _activations = 0;
        ActivateEvent?.Invoke();
        return _activations > 0;
    }
    
    public virtual void OnEnter(MapObject mapObject) {
        if (mapObject is not THandler handler) return;
        ActiveElements++;
        _handlers ??= new List<THandler>();
        _handlers.Add(handler);
        if (_handlers.Count == 1) ActivateEvent += OnActivate;
    }

    public virtual void OnExit(MapObject mapObject) {
        if (mapObject is not THandler handler) return;
        ActiveElements--;
        _handlers?.Remove(handler);
        if (_handlers is { Count: 0 }) ActivateEvent -= OnActivate;
    }

    void OnActivate() => Activate(_handlers.ToArray());
}