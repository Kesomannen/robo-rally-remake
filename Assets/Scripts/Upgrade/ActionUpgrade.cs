using System;
using UnityEngine;

public abstract class ActionUpgrade : ScriptableObject {
    [SerializeField] UseContext _useContext;
    
    public UseContext UseContext => _useContext;
}