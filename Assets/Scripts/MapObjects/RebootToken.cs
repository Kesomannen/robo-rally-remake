using System.Collections;
using UnityEngine;

public class RebootToken : StaticObject {
    [SerializeField] Vector2Int _direction;

    void DoRespawning(PlayerModel player) {
        Scheduler.Push(Routine());

        IEnumerator Routine() {
            Debug.Log($"Respawning {player.Owner}", this);

            if (CurrentDynamic != null) {
                yield return Interaction.PushRoutine(CurrentDynamic, RotateAsObject(_direction));
            }

            MapSystem.Instance.MoveObjectInstant(player, GridPos);
        }
    }
    
    public static void Respawn(PlayerModel player) {
        var token = MapSystem.Instance.GetBoard(player).RebootToken;
        token.DoRespawning(player);
    }
}