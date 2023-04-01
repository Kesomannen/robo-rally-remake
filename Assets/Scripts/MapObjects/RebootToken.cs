using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class RebootToken : MapObject, ITooltipable, IPointerClickHandler {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;
    [SerializeField] Optional<OverlayData<Choice<Vector2Int>>> _directionChoiceOverlay;

    Vector2Int _startPos;
    
    public static event Action<RebootToken> RebootTokenClicked; 

    public string Header => _isSpawnPoint ? "Spawn Point" : "Reboot Token";
    public string Description {
        get {
            if (_isSpawnPoint) {
                return "Robots spawn here at the start of the game.";
            }
            var playerBoard = MapSystem.GetParentBoard(PlayerSystem.LocalPlayer.Model);
            var board = MapSystem.GetParentBoard(this);
            return playerBoard == board ? "You will respawn here if you reboot." : "Robots rebooting on this board will respawn here.";
        }
    }

    public bool IsSpawnPoint => _isSpawnPoint;

    public IEnumerator RespawnRoutine(IPlayer player) {
        var obj = player.Object;
        var obstructions = MapSystem.GetTile(GridPos).OfType<ICanEnterHandler>().Where(o => o.Object != obj).ToArray();
        
        if (obstructions.Length == 1) {
            if (Interaction.Push(obstructions[0].Object, _direction, out var moveAction)) {
                yield return Interaction.EaseEvent(moveAction);
            } else {
                Debug.LogWarning("RebootToken is obstructed!", this);
            }    
        } else if (obstructions.Length > 1) {
            Debug.LogWarning("RebootToken is obstructed!", this);
        }
        
        MapSystem.Instance.MoveObjectInstant(obj, GridPos);
        player.Owner.Model.OnRespawn();

        int targetRot;
        if (_directionChoiceOverlay.Enabled) {
            var result = new Vector2Int[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<Vector2Int> {
                Overlay = _directionChoiceOverlay.Value,
                Player = player.Owner,
                Options = DirectionChoice.DirectionsWithoutZero,
                Message = "choosing a direction to respawn",
                OutputArray = result,
                MinChoices = 1
            });
            targetRot = VectorHelper.GetRotationSteps(result[0]);
        } else {
            targetRot = VectorHelper.GetRotationSteps(_direction);
        }
        yield return obj.RotateRoutine(targetRot - obj.Rotator.RotZ);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) return;
        RebootTokenClicked?.Invoke(this);
    }
}