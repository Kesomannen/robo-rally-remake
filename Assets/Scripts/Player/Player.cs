using UnityEngine;

public class Player {
   public PlayerModel Model { get; private set; }

    public Player() {
        Model = MapSystem.instance.CreateMapObject(
            PlayerManager.instance.PlayerModelPrefab,
            MapSystem.instance.GetRandomEmptyGridPos()
        ).GetComponent<PlayerModel>();
    }
}