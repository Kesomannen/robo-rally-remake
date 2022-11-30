using System.Collections;
using UnityEngine;

public abstract class ProgramCardData : Lookup<ProgramCardData>, IContainable<ProgramCardData>, ITooltipable {
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [SerializeField] Sprite _artwork;
    [SerializeField] CardType _type = CardType.Action;

    public string Name => _name;
    public string Header => Name;
    public string Description => _description;
    public Sprite Artwork => _artwork;
    public CardType Type => _type;

    public Container<ProgramCardData> DefaultContainerPrefab => GameSettings.Instance.ProgramCardContainerPrefab;

    public abstract IEnumerator ExecuteRoutine(Player player, int positionInRegister);
    public abstract bool CanPlace(Player player, int positionInRegister);

    public virtual void OnDraw(Player player) { }
    public virtual void OnDiscard(Player player) { }

    public override string ToString() => _name;
    
    public enum CardType {
        Action,
        Damage,
        Utility,
    }
}