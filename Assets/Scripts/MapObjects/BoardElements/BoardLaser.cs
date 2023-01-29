using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardLaser : MapObject, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] Laser _laserPrefab;
    [SerializeField] CardAffector _affector;
    [SerializeField] ParticleSystem _damageParticles;
    [SerializeField] SoundEffect _damageSound, _laserSound;
    
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
            l.SetActiveVisual(false);
            l.OnObstructed += OnObstructed;
            l.OnUnobstructed += OnUnobstructed;
        });
    }

    static int _hits;
    
    public static bool ActivateElement() {
        if (_lasersInScene == 0) return false;
        _hits = 0;
        OnActivate?.Invoke();
        return _hits > 0;
    }

    void Activate() {
        foreach (var target in _targets) {
            if (target is not IPlayer player) continue;
            TaskScheduler.PushRoutine(Damage(player));
            _hits++;
        }

        IEnumerator Damage(IPlayer target) {
            _lasers.ForEach(l => l.SetActiveVisual(true));

            _damageSound.Play();
            var pos = _damageParticles.transform.position;
            _damageParticles.transform.position = target.Owner.Model.transform.position;
            Debug.Log($"Moved particles from {pos} to {_damageParticles.transform.position}, target pos: {target.Owner.Model.transform.position}");
            _damageParticles.Play();
            yield return CoroutineUtils.Wait(Mathf.Max(_damageSound.Clip.length, _damageParticles.main.duration) + 0.5f);

            _lasers.ForEach(l => l.SetActiveVisual(false));
            _affector.Apply(target);
        }
    }

    void OnUnobstructed(Laser laser, MapObject obj) {
        if (!_targets.Contains(obj)) return;
        
        _targets.Remove(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++) {
            _lasers[i].gameObject.SetActive(true);
        }
    }
    
    void OnObstructed(Laser laser, MapObject obj) {
        if (_targets.Contains(obj)) return;
        
        _targets.Add(obj);
        var index = _lasers.IndexOf(laser);
        for (var i = index + 1; i < _lasers.Count; i++) {
            _lasers[i].gameObject.SetActive(false);
        }
    }
}