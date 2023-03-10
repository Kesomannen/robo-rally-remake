using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAffector", menuName = "ScriptableObjects/Affectors/Damage")]
public class DamageAffector : ScriptableAffector<Player> {
    [SerializeField] Target _target;
    [SerializeField] ProgramCardData[] _cards;
    [SerializeField] ProgramCardData _replacementCard;
    [SerializeField] bool _replace;
    [SerializeField] Destination _destination;
    [SerializeField] Placement _placement;

    Pile Pile => _destination.Convert<Destination, Pile>(-1);
    CardPlacement CardPlacement => _placement.Convert<Placement, CardPlacement>(-1);

    public override void Apply(Player target) {
        var cardAffector = GetDamage(target);
        
        if (_replace) cardAffector.Cards.Clear();
        cardAffector.Cards.AddRange(_cards);

        cardAffector.Destination = EnumUtils.ModifyValue(cardAffector.Destination, Pile);
        cardAffector.Placement = EnumUtils.ModifyValue(cardAffector.Placement, CardPlacement);
    }
    
    public override void Remove(Player target) {
        var cardAffector = GetDamage(target);

        foreach (var card in _cards) {
            if (cardAffector.Cards.Remove(card) && _replacementCard != null) {
                cardAffector.Cards.Add(_replacementCard);
            }
        }

        if ((int)cardAffector.Destination == (int)_destination) cardAffector.Destination = default;
        if ((int)cardAffector.Placement == (int)_placement) cardAffector.Placement = default;
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