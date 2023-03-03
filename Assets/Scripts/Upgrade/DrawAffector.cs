using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawAffector", menuName = "ScriptableObjects/Affectors/Draw")]
public class DrawAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] Type _type;
    [SerializeField] int _amount;
    
    public override void Apply(IPlayer player) {
        switch (_type) {
            case Type.Constant: player.Owner.DrawCards(_amount); break;
            case Type.UpTo: player.Owner.DrawCardsUpTo(_amount); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    enum Type {
        Constant,
        UpTo
    }
}