using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Highlight))]
public class PlayerModel : MapObject, IPlayer, ICanEnterExitHandler, ITooltipable {
    [Header("Prefabs")]
    [SerializeField] Laser _laserPrefab;
    [SerializeField] ParticleSystem _hitParticle;
    [SerializeField] ParticleSystem _moveParticle;
    
    [Header("Tween")]
    [SerializeField] LeanTweenType _moveTweenType;
    [SerializeField] float _moveTweenSpeed;
    
    [Header("Audio")]
    [SerializeField] SoundEffect _moveSound;
    [SerializeField] SoundEffect _rotateSound;
    [SerializeField] SoundEffect _laserSound;
    [SerializeField] SoundEffect _rebootSound;

    Highlight _highlight;

    public readonly List<Type> IgnoredObjectsForMoving = new();
    public readonly List<Type> IgnoredObjectsForLaser = new();
    public readonly List<Vector2Int> LaserDirections = new() { Vector2Int.right };

    public Player Owner { get; private set; }
    public RebootToken Spawn { get; private set; }

    public bool Movable { get; set; } = true;
    protected override bool CanRotate => true;
    
    public event Action<CallbackContext> OnPush;
    public event Action<CallbackContext> OnShoot; 

    public struct CallbackContext {
        public Player Target;
        public Player Attacker;
        public CardAffector Affector;
        public Vector2Int OutgoingDirection;
    }
    
    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;
    
    public string Header => PlayerSystem.IsLocal(Owner) ? Owner + " (You)" : Owner.ToString();
    public string Description => null;

    public override void Fall(IBoard board) {
        _rebootSound.Play();
        Owner.Reboot(board);
    }

    protected override void Awake() {
        base.Awake();
        _highlight = GetComponent<Highlight>();
        OnRotationChanged += _ => _rotateSound.Play();
    }

    public void Init(Player owner, RebootToken spawnPoint) {
        Owner = owner;
        Spawn = spawnPoint;
        GetComponent<SpriteRenderer>().sprite = owner.RobotData.Sprite;
    }

    static int _hits;
    
    public static bool ShootLasers() {
        _hits = 0;
        foreach (var model in PlayerSystem.Players.Where(p => !p.IsRebooted.Value).Select(p => p.Model)) {
            model.ShootLaser();
        }
        return _hits > 0;
    }
    
    void ShootLaser(int maxDistance = 20) {
        foreach (var dir in LaserDirections.Select(dir => Rotator.Rotate(dir))) {
            var pos = GridPos;
            for (var i = 0; i < maxDistance; i++) {
                if (!Interaction.CanMove(pos, dir, this, IgnoredObjectsForLaser)) {
                    continue;
                }
                pos += dir;
            }

            var targetFilled = MapSystem.TryGetTile(pos + dir, out var tile);
            if (!targetFilled) continue;
        
            var hits = tile.OfType<IPlayer>().Where(p => p.Owner != Owner).ToArray();
            if (hits.Length == 0) continue;
            _hits += hits.Length;

            TaskScheduler.PushRoutine(Fire(dir, hits));   
        }

        IEnumerator Fire(Vector2Int direction, IEnumerable<IPlayer> hits) {
            var lasers = Laser.ShootLaser(_laserPrefab, this, direction, maxDistance, ignoredTypes: IgnoredObjectsForLaser);
            lasers.ForEach(l => l.SetActiveVisual(true));
            _laserSound.Play();
            
            foreach (var player in hits) {
                player.Owner.ApplyCardAffector(Owner.LaserAffector);
                OnShoot?.Invoke(new CallbackContext {
                    Target = player.Owner,
                    Attacker = Owner,
                    Affector = Owner.LaserAffector,
                    OutgoingDirection = direction
                });
                
                _hitParticle.transform.position = player.Owner.Model.transform.position;
                _hitParticle.Play();
                yield return CoroutineUtils.Wait(_hitParticle.main.duration);
            }
            
            lasers.ForEach(l => MapSystem.DestroyObject(l, false));
        }
    }

    public IEnumerator Move(Vector2Int dir, bool relative) {
        if (Owner.IsRebooted.Value) yield break;
        
        var moveVector = relative ? Rotator.Rotate(dir) : dir;
        if (!Interaction.Push(this, moveVector, out var mapEvent, IgnoredObjectsForMoving)) yield break;
        
        _moveParticle.Play();
        _moveSound.Play();
        
        RegisterPush(mapEvent);
        yield return Interaction.EaseEvent(mapEvent, _moveTweenType, _moveTweenSpeed);
    }
    
    public void RegisterPush(MapEvent mapEvent) {
        if (Owner.PushAffector.Cards.Count <= 0) return;

        foreach (var player in mapEvent.MapObjects.OfType<IPlayer>().Select(m => m.Owner).Where(p => p != Owner)) {
            player.ApplyCardAffector(Owner.PushAffector);
            OnPush?.Invoke(new CallbackContext {
                Target = player,
                Attacker = Owner,
                Affector = Owner.PushAffector,
                OutgoingDirection = mapEvent.Direction
            });
        }
    }

    public void Highlight(bool highlight) {
        _highlight.enabled = highlight;
    }
}