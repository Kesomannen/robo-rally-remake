using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerModel : MapObject, IPlayer, ICanEnterExitHandler, ITooltipable, IPointerClickHandler {
    [SerializeField] Laser _laserPrefab;
    
    public Player Owner { get; private set; }

    public bool Pushable => true;
    public override bool CanRotate => true;

    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;

    public string Header => Owner.ToString();
    public string Description => null;

    public override void Fall(IBoard board) {
        Owner.Reboot(board);
    }

    public void Init(Player owner) {
        Owner = owner;
    }

    public IEnumerator FireLaser(Vector2Int dir) {
        var lasers = Laser.ShootLaser(_laserPrefab, this, dir);
        if (lasers.Count == 0) yield break;
        // We only need to check the last laser in the chain
        var hits = MapSystem.GetTile(lasers.Last().GridPos).OfType<IPlayer>().ToArray();
        foreach (var hit in hits){
            Owner.LaserDamage.Apply(hit.Owner);
        }
        yield return Helpers.Wait(0.5f);
        lasers.ForEach(l => MapSystem.Instance.DestroyObject(l, false));
    }
    
    public void OnPointerClick(PointerEventData e) {
        Debug.Log(Owner);
    }
}