using UnityEngine;

// Serializable version of a permanent affector
public abstract class ScriptablePermanentAffector<T> : ScriptableObject, IPermanentAffector<T> {
    public abstract void Apply(T target);
}