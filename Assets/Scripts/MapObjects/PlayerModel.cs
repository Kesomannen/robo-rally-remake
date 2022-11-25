using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerModel : MapObject, IPlayer, ICanEnterExitHandler, ITooltipable {
    [SerializeField] Laser _laserPrefab;
    
    public Player Owner { get; private set; }

    public bool Pushable => true;
    protected override bool CanRotate => true;

    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;

    // Add you after header if player is local
    public string Header => PlayerManager.IsLocal(Owner) ? Owner + " (You)" : Owner.ToString();
    public string Description => null;

    public override void Fall(IBoard board) {
        Owner.Reboot(board);
    }

    public void Init(Player owner) {
        Owner = owner;
        GetComponent<SpriteRenderer>().sprite = owner.RobotData.Sprite;
    }

    public IEnumerator FireLaser(Vector2Int dir) {
        var lasers = Laser.ShootLaser(_laserPrefab, this, dir);
        if (lasers.Count == 0) yield break;
        
        // We only need to check the last laser in the chain
        var hits = MapSystem.GetTile(lasers.Last().GridPos).OfType<IPlayer>().ToArray();
        foreach (var hit in hits){
            Owner.LaserDamage.Apply(hit.Owner);
        }
        
        yield return Helpers.Wait(1f);
        lasers.ForEach(l => MapSystem.Instance.DestroyObject(l, false));
    }
}