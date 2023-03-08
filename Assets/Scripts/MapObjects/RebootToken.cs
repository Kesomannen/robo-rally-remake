using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class RebootToken : MapObject, ITooltipable {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;
    [SerializeField] OverlayData<Choice<Vector2Int>> _directionChoiceOverlay;

    Vector2Int _startPos;

    public bool IsSpawnPoint => _isSpawnPoint;
    
    public string Header => _isSpawnPoint ? "Spawn Point" : "Reboot Token";
    public string Description {
        get {
            if (_isSpawnPoint){
                return "Robots spawn here at the start of the game.";
            }
            var playerBoard = MapSystem.GetParentBoard(PlayerSystem.LocalPlayer.Model);
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

    public IEnumerator RespawnRoutine(IPlayer player) {
        var obj = player.Object;
        var tile = MapSystem.GetTile(GridPos);
        var obstructions = tile.OfType<ICanEnterHandler>().Where(o => o.Object != obj);
        foreach (var obstruct in obstructions) {
            if (Interaction.Push(obstruct.Object, _direction, out var moveAction)) {
                yield return Interaction.EaseEvent(moveAction);
            } else {
                Debug.LogWarning("RebootToken is obstructed!", this);
            }
        }
        MapSystem.Instance.MoveObjectInstant(obj, GridPos);

        var result = new Vector2Int[1];
        yield return ChoiceSystem.DoChoice(new ChoiceData<Vector2Int> {
            Overlay = _directionChoiceOverlay,
            Player = player.Owner,
            Options = DirectionChoice.DirectionsWithoutZero,
            Message = "choosing a direction to respawn",
            OutputArray = result,
            MinChoices = 1
        });
        var targetRot = VectorHelper.GetRotationSteps(result[0]);
        yield return obj.RotateRoutine(targetRot - obj.Rotator.RotZ);
    }
}