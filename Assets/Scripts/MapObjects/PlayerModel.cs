using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Highlight))]
public class PlayerModel : MapObject, IPlayer, ICanEnterExitHandler, ITooltipable {
    [Header("Prefabs")]
    [SerializeField] Laser _laserPrefab;

    Highlight _highlight;
    
    public Player Owner { get; private set; }

    public bool Pushable => true;
    protected override bool CanRotate => true;

    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;
    
    public string Header => PlayerManager.IsLocal(Owner) ? Owner + " (You)" : Owner.ToString();
    public string Description => null;

    public override void Fall(IBoard board) {
        Owner.Reboot(board);
    }

    protected override void Awake() {
        base.Awake();
        _highlight = GetComponent<Highlight>();
    }

    public void Init(Player owner) {
        Owner = owner;
        GetComponent<SpriteRenderer>().sprite = owner.RobotData.Sprite;
    }

    public IEnumerator FireLaser(Vector2Int dir) {
        var lasers = Laser.ShootLaser(_laserPrefab, this, dir);
        if (lasers.Count == 0) yield break;
        
        // We check one step ahead of last laser
        var targetTileFilled = MapSystem.TryGetTile(lasers.Last().GridPos + dir, out var hitTile);
        if (targetTileFilled) {
            var hits = hitTile.OfType<IPlayer>().ToArray();
            foreach (var hit in hits){
                Owner.LaserAffector.Apply(hit.Owner);
            }   
        }

        yield return CoroutineUtils.Wait(1f);
        lasers.ForEach(l => MapSystem.Instance.DestroyObject(l, false));
    }

    public IEnumerator Move(Vector2Int dir, bool relative) {
        if (Owner.IsRebooted.Value) yield break;
        
        var moveVector = relative ? Rotator.Rotate(dir) : dir;
        if (Interaction.Push(this, moveVector, out var mapEvent)){
            yield return Interaction.EaseEvent(mapEvent);
        }
    }

    public void Highlight(bool highlight) {
        _highlight.enabled = highlight;
    }
}