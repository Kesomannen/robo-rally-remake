using System.Collections;
using UnityEngine;

public abstract class ProgramCardData : Lookup<ProgramCardData>, ITooltipable {
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [SerializeField] Sprite _artwork;
    [SerializeField] CardType _type = CardType.Action;

    public string Name => _name;
    public string Header => Name;
    public string Description => _description;
    public Sprite Artwork => _artwork;
    public CardType Type => _type;

    public abstract IEnumerator ExecuteRoutine(Player player, int register);
    public abstract bool CanPlace(Player player, int register);

    public override string ToString() => _name;
    
    public enum CardType {
        Action,
        Damage,
        Utility,
    }
}