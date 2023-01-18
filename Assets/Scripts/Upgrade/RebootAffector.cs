using UnityEngine;

[CreateAssetMenu(fileName = "RebootAffector", menuName = "ScriptableObjects/Affectors/Reboot")]
public class RebootAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] bool _takeDamage;

    public override void Apply(IPlayer player) {
        player.Owner.RebootFromParentBoard(_takeDamage);
    }
}