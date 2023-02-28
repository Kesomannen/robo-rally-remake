using System.Collections;
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

    Highlight _highlight;
    
    public Player Owner { get; private set; }
    public RebootToken Spawn { get; private set; }

    public bool Pushable => true;
    protected override bool CanRotate => true;

    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;
    
    public string Header => PlayerSystem.IsLocal(Owner) ? Owner + " (You)" : Owner.ToString();
    public string Description => null;

    public override void Fall(IBoard board) {
        Owner.Reboot(board);
    }

    protected override void Awake() {
        base.Awake();
        _highlight = GetComponent<Highlight>();
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
            model.ShootLaser(model.Rotator.Identity);
        }
        return _hits > 0;
    }
    
    void ShootLaser(Vector2Int dir, int maxDistance = 20) {
        var pos = GridPos;
        for (var i = 0; i < maxDistance; i++) {
            if (!Interaction.CanMove(pos, dir, this)) {
                break;
            }
            pos += dir;
        }

        var targetFilled = MapSystem.TryGetTile(pos + dir, out var tile);
        if (!targetFilled) return;
        
        var hits = tile.OfType<IPlayer>().Where(p => p.Owner != Owner).ToArray();
        if (hits.Length == 0) return;
        _hits += hits.Length;

        var lasers = Laser.ShootLaser(_laserPrefab, this, dir, maxDistance);
        lasers.ForEach(l => l.SetActiveVisual(true));
        for (var i = 0; i < hits.Length; i++) {
            TaskScheduler.PushRoutine(DamagePlayer(hits[i], i == hits.Length - 1));
        }

        IEnumerator DamagePlayer(IPlayer player, bool destroyLasers) {
            player.Owner.ApplyCardAffector(Owner.LaserAffector);
            _hitParticle.transform.position = player.Owner.Model.transform.position;
            _hitParticle.Play();
            yield return CoroutineUtils.Wait(_hitParticle.main.duration);
            
            if (destroyLasers) {
                yield return CoroutineUtils.Wait(0.5f);
                lasers.ForEach(l => MapSystem.DestroyObject(l, false));
            }
        }
    }

    public IEnumerator Move(Vector2Int dir, bool relative) {
        if (Owner.IsRebooted.Value) yield break;
        
        var moveVector = relative ? Rotator.Rotate(dir) : dir;
        if (!Interaction.Push(this, moveVector, out var mapEvent)) yield break;
        
        _moveParticle.Play();
        yield return Interaction.EaseEvent(mapEvent, _moveTweenType, _moveTweenSpeed);
    }

    public void Highlight(bool highlight) {
        _highlight.enabled = highlight;
    }
}