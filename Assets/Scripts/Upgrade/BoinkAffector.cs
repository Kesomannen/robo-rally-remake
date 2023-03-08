using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Boink")]
public class BoinkAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] OverlayData<Choice<Vector2Int>> _directionChoiceOverlay;

    public override void Apply(IPlayer player) {
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new Vector2Int[1];
            var model = player.Owner.Model;

            yield return ChoiceSystem.DoChoice(new ChoiceData<Vector2Int> {
                Overlay = _directionChoiceOverlay,
                Player = player.Owner,
                Options = DirectionChoice.DirectionsWithoutZero,
                Message = "choosing a direction to move with Boink",
                OutputArray = result,
                MinChoices = 1
            });
            yield return model.Move(result[0], false);
        }
    }
}