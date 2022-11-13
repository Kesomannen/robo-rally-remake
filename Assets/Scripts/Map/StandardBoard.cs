using UnityEngine;

public class StandardBoard : BaseBoard {
    [SerializeField] RebootToken _rebootToken;

    public override void Respawn(PlayerModel obj) {
        if (obj is PlayerModel player) {
            Scheduler.Push(_rebootToken.RespawnRoutine(player), $"StandardBoard Respawn {player}");
        } else {
            Debug.LogWarning($"Respawn not implemented for {obj.name}");
        }
    }
}