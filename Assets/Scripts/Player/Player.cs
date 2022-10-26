using UnityEngine;

public class Player {
   public PlayerModel Model { get; private set; }

    public Player(Vector2Int gridPos) {
        Model = MapSystem.instance.CreateMapObject(
            PlayerManager.instance.PlayerModelPrefab,
            gridPos
        ) as PlayerModel;
    }
}