using UnityEngine;

[CreateAssetMenu(fileName = "CardAffector", menuName = "ScriptableObjects/Affectors/Card")]
public class ScriptableCardAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] ProgramCardData[] _cards;
    [SerializeField] Pile _destination;
    [SerializeField] CardPlacement _placement;
    
    public override void Apply(IPlayer player) =>
        CardAffector.Apply(player.Owner, _cards, _destination, _placement);
    
    public CardAffector ToInstance() => new(_cards, _destination, _placement);
}