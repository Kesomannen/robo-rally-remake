using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCleanerProgram", menuName = "ScriptableObjects/Programs/Card Cleaner")]
public class CardCleanerProgram : ProgramCardData {
    [SerializeField] int _maxCards;
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] Pile _pile;
    
    public override bool CanPlace(Player player, int register) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int register) {
        var collection = player.GetCollection(_pile);
        for (var i = 0; i < _maxCards; i++) {
            collection.RemoveCard(_targetCard);
        }
        yield break;
    }
}