using System.Collections;
using System.Linq;
using UnityEngine;

public class RebootToken : MapObject, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;

    Vector2Int _startPos;

    public bool IsSpawnPoint => _isSpawnPoint;
    
    public string Header => _isSpawnPoint ? "Spawn Point" : "Reboot Token";
    public string Description {
        get {
            if (_isSpawnPoint){
                return "Robots spawn here at the start of the game. Rebooting on the start board respawns you here.";
            }
            var playerBoard = MapSystem.GetParentBoard(PlayerManager.LocalPlayer.Model);
            var board = MapSystem.GetParentBoard(this);
            return playerBoard == board ? "You will respawn here if you reboot." : "Robots rebooting on this board will respawn here.";
        }
    }

    void Start() {
        _startPos = GridPos;
    }

    public override void Fall(IBoard board) {
        MapSystem.Instance.MoveObjectInstant(Object, _startPos);
    }

    public IEnumerator RespawnRoutine(MapObject obj) {
        var tile = MapSystem.GetTile(GridPos);
        var obstructions = tile.OfType<ICanEnterHandler>();
        foreach (var obstruct in obstructions) {
            if (Interaction.Push(obstruct.Object, _direction, out var moveAction)) {
                yield return Interaction.EaseEvent(moveAction);
            } else {
                Debug.LogWarning($"RebootToken is obstructed!", this);
            }
        }
        // TODO: Allow the player to choose direction
        MapSystem.Instance.MoveObjectInstant(obj, GridPos);
    }
}