using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCleanerProgram", menuName = "ScriptableObjects/Programs/Card Cleaner")]
public class CardCleanerProgram : ProgramCardData {
    [SerializeField] int _maxCards;
    [SerializeField] ProgramCardData _targetCard;
    [SerializeField] Pile _pile;
    
    public override bool CanPlace(Player player, int positionInRegister) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        var collection = player.GetCollection(_pile);
        for (var i = 0; i < _maxCards; i++) {
            if (!collection.RemoveCard(_targetCard)) yield break;
        }
    }
}