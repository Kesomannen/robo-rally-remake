using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardLaser : MapObject, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] Laser _laserPrefab;
    [SerializeField] CardAffector _affector;
    
    List<Laser> _lasers;
    readonly List<MapObject> _targets = new();

    static event Action OnActivate;

    public string Header => "Laser";
    public string Description => $"Ending a register in this laser deals {_affector.Cards.Count} damage.";

    protected override void Awake(){
        base.Awake();
        _direction = Rotator.Rotate(_direction);
        OnRotationChanged += r => {
            _direction = _direction.Transform(r);
        };
        OnActivate += Activate;
    }
    
    void Start() {
        _lasers = Laser.ShootLaser(_laserPrefab, this, _direction, ignoreSource: false);
        _lasers.ForEach(l => {
            l.OnObstructed += OnObstructed;
            l.OnUnobstructed += OnUnobstructed;
        });
    }

    public static IEnumerator ActivateElement(){
        OnActivate?.Invoke();
        yield return Scheduler.WaitUntilClearRoutine();
    }

    void Activate(){
        foreach (var target in _targets){
            if (target is not IPlayer player) continue;
            _affector.Apply(player);
        }
    }

    void OnUnobstructed(Laser laser, MapObject obj){
        _targets.Remove(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++){
            _lasers[i].gameObject.SetActive(true);
        }
    }
    
    void OnObstructed(Laser laser, MapObject obj){
        _targets.Add(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++){
            _lasers[i].gameObject.SetActive(false);
        }
    }
}