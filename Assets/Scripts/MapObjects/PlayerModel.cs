using UnityEngine;

public class PlayerModel : MapObject, IPlayer, ICanEnterHandler, IPushable {
    public Player Owner { get; private set; }

    public bool CanEnter(Vector2Int enterDir) => false;

    public void Init(Player owner) {
        Owner = owner;
    }

    public void OnPush(Vector2Int dir, MapObject source) { }
}