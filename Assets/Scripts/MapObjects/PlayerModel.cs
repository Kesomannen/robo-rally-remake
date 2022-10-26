using UnityEngine;

public class PlayerModel : MapObject {
    public override bool IsStatic => false;
    public override bool CanEnter(Vector2Int dir) => false;

    void Update() {
    }
}