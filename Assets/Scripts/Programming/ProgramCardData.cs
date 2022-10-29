using System.Collections;
using UnityEngine;

public abstract class ProgramCardData : ScriptableObject {
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [SerializeField] Sprite _artwork;
    [SerializeField] CardType _type = CardType.Action;

    public string Name => _name;
    public string Description => _description;
    public Sprite Artwork => _artwork;
    public CardType Type => _type;

    public abstract IEnumerator Execute(Player player, int positionInRegister);
    public abstract bool CanPlace(Player player, int positionInRegister);

    public override string ToString() => _name;
    
    public enum CardType {
        Action,
        Damage,
        Utility,
    }
}