using UnityEngine;

// Serializable version of an affector
public abstract class ScriptableAffector<T> : ScriptablePermanentAffector<T>, IAffector<T> {
    public abstract void Remove(T target);
}