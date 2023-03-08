using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Smash Bot")]
public class SmashBotAffector : ScriptableAffector<IPlayer> {
    public override void Apply(IPlayer target) {
        target.Owner.Model.OnPush += OnPush;
    }
    
    public override void Remove(IPlayer target) {
        target.Owner.Model.OnPush -= OnPush;
    }

    static void OnPush(PlayerModel.CallbackContext context) {
        if (Interaction.Push(context.Target.Model, context.OutgoingDirection, out var mapEvent)) {
            Interaction.EaseEvent(mapEvent);
        }
    }
}