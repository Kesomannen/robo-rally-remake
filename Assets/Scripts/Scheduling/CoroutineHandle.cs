using System;
using System.Collections;
using UnityEngine;

public class CoroutineHandle : IEnumerator {
    public event Action<CoroutineHandle> OnComplete;
    public bool IsDone { get; private set; }
    
    public bool MoveNext() => !IsDone;
    public object Current { get; }
    public void Reset() { }
    
    public CoroutineHandle(MonoBehaviour owner, IEnumerator routine) {
        Current = owner.StartCoroutine(Execute(routine));
    }

    IEnumerator Execute(IEnumerator routine) {
        yield return routine;
        IsDone = true;
        OnComplete?.Invoke(this);
    }
}

public static class CoroutineHandleExtensions {
    public static CoroutineHandle RunCoroutine(this MonoBehaviour owner, IEnumerator routine) {
        return new CoroutineHandle(owner, routine);
    }
}