using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAffector", menuName = "ScriptableObjects/Affectors/Damage")]
public class DamageAffector : ScriptableAffector<IPlayer> {
    [SerializeField] Target _target;
    [SerializeField] ProgramCardData[] _cards;
    [SerializeField] ProgramCardData _replacementCard;
    [SerializeField] bool _replace;
    [SerializeField] Destination _destination;
    [SerializeField] Placement _placement;

    public override void Apply(IPlayer target) {
        var player = target.Owner;
        var damage = GetDamage(player);
        
        if (_replace) damage.Cards.Clear();
        damage.Cards.AddRange(_cards);

        damage.Destination = CoroutineUtils.ModifyEnumValue(damage.Destination, _destination);
        damage.Placement = CoroutineUtils.ModifyEnumValue(damage.Placement, _placement);
    }
    
    public override void Remove(IPlayer target) {
        var player = target.Owner;
        var damage = GetDamage(player);

        foreach (var card in _cards){
            if (damage.Cards.Remove(card) && _replacementCard != null) {
                damage.Cards.Add(_replacementCard);
            }
        }

        if ((int)damage.Destination == (int)_destination) damage.Destination = default;
        if ((int)damage.Placement == (int)_placement) damage.Placement = default;
    }
    
    CardAffector GetDamage(Player player) {
        return _target switch {
            Target.Reboot => player.RebootAffector,
            Target.Laser => player.LaserAffector,
            Target.Push => player.PushAffector,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    enum Target {
        Reboot,
        Laser,
        Push,
    }
    
    enum Destination {
        Unchanged = 0,
        Hand = 3,
        DrawPile = 2,
        DiscardPile = 1,
    }
    
    enum Placement {
        Unchanged = 0,
        Top = 1,
        Bottom = 2,
        Random = 3,
    }
}