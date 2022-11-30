using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCardData", menuName = "ScriptableObjects/UpgradeData")]
public class UpgradeCardData : ScriptableObject, IContainable<UpgradeCardData> {
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [SerializeField] Sprite _icon;
    [SerializeField] int _cost;
    [SerializeField] bool _isAction;

    [SerializeField] List<ScriptableAffector<IPlayer>> _temporaryAffectors;
    [SerializeField] List<ScriptablePermanentAffector<IPlayer>> _permanentAffectors;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public bool IsAction => _isAction;

    public Container<UpgradeCardData> DefaultContainerPrefab => GameSettings.Instance.UpgradeContainerPrefab;

    public void Apply(IPlayer player) {
        if (_isAction){
            foreach (var affector in _permanentAffectors) affector.Apply(player);
        } else {
            foreach (var affector in _temporaryAffectors) affector.Apply(player);
        }
    }
    
    public void Remove(IPlayer player) {
        if (_isAction){
            throw new InvalidOperationException("Cannot remove an action upgrade");
        } 
        foreach (var affector in _temporaryAffectors) {
            affector.Remove(player);
        }
    }
}

[Flags]
public enum UseContext {
    None = 0,
    DuringProgramming = 1,
    AfterLockIn = 2,
    OwnRegister = 4,
    OtherRegisters = 8,
    BoardElements = 16,
    Shop = 32,
}