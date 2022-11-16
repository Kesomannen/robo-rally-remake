using UnityEngine;

public class PlayerModel : MapObject, IPlayer, ICanEnterExitHandler {
    public Player Owner { get; private set; }

    public bool Pushable => true;

    public bool CanEnter(Vector2Int enterDir) => false;
    public bool CanExit(Vector2Int exitDir) => true;

    public override void Fall(IBoard board) {
        Owner.Reboot(board);
    }

    public void Init(Player owner) {
        Owner = owner;
    }
}