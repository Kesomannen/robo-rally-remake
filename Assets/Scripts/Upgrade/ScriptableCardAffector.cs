using UnityEngine;

[CreateAssetMenu(fileName = "CardAffector", menuName = "ScriptableObjects/Affectors/Card")]
public class ScriptableCardAffector : ScriptablePermanentAffector<Player> {
    [SerializeField] ProgramCardData[] _cards;
    [SerializeField] Pile _destination;
    [SerializeField] CardPlacement _placement;
    
    public override void Apply(Player player) {
        ToInstance().Apply(player);
    }

    public CardAffector ToInstance() => new(_cards, _destination, _placement);
}