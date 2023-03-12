using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Smash Bot")]
public class SmashBotAffector : ScriptableAffector<Player> {
    public override void Apply(Player target) {
        target.Model.OnPush += OnPush;
    }
    
    public override void Remove(Player target) {
        target.Model.OnPush -= OnPush;
    }

    static void OnPush(PlayerModel.CallbackContext context) {
        if (Interaction.Push(context.Target.Model, context.OutgoingDirection, out var mapEvent)) {
            TaskScheduler.PushRoutine(Interaction.EaseEvent(mapEvent));
        }
    }
}