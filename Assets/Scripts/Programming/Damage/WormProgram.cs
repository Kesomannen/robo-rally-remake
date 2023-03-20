using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Program/Worm")]
public class WormProgram : ProgramCardData {
    [SerializeField] bool _takeDamage;
    
    public override IEnumerator ExecuteRoutine(Player player, int register) {
        player.RebootFromParentBoard(_takeDamage);
        yield break;
    }

    public override bool CanPlace(Player player, int register) => true;
}