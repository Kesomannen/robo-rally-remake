using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardLaser : MapObject, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] Laser _laserPrefab;
    [SerializeField] CardAffector _affector;
    
    List<Laser> _lasers;
    readonly List<MapObject> _targets = new();
    static int _lasersInScene;

    static event Action OnActivate;

    public string Header => "Laser";
    public string Description {
        get {
            var cards = _affector.Cards;
            var count = cards.Count;
            var word = count == 1 ? "card" : "cards";
            return $"Ending a register in this laser deals {count} {cards[0].Name} {word}.";
        }
    }

    protected override void Awake() {
        base.Awake();
        _lasersInScene++;
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

    public static bool ActivateElement() {
        if (_lasersInScene == 0) return false;
        OnActivate?.Invoke();
        return true;
    }

    void Activate() {
        foreach (var target in _targets){
            if (target is not IPlayer player) continue;
            _affector.Apply(player);
        }
    }

    void OnUnobstructed(Laser laser, MapObject obj) {
        _targets.Remove(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++) {
            _lasers[i].gameObject.SetActive(true);
        }
    }
    
    void OnObstructed(Laser laser, MapObject obj) {
        _targets.Add(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++) {
            _lasers[i].gameObject.SetActive(false);
        }
    }
}