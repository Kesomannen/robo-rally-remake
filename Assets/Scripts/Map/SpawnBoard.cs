using UnityEngine;

public class SpawnBoard : BaseBoard {
    public override void Respawn(DynamicObject obj) {
        if (obj is PlayerModel player) {
            var spawnPoint = PlayerManager.GetSpawnPoint(player.Owner);
            Scheduler.Push(spawnPoint.RespawnRoutine(player));
        } else {
            Debug.LogWarning($"Respawn not implemented for {obj.name}");
        }
    }
}