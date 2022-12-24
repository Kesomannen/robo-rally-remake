using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ChoiceProgram", menuName = "ScriptableObjects/Programs/Choice")]
public class ChoiceProgram : ProgramCardData {
    [SerializeField] ProgramCardData[] _options;
    [SerializeField] OverlayData<ProgramCardChoice> _choiceOverlay;

    bool _hasMadeChoice;
    IEnumerator _choiceRoutine;

    public override bool CanPlace(Player player, int positionInRegister) => _options.Any(c => c.CanPlace(player, positionInRegister));

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        _hasMadeChoice = false;

        var availableArray = _options.Select(c => c.CanPlace(player, positionInRegister)).ToArray();
        var obj = OverlaySystem.Instance.PushAndShowOverlay(_choiceOverlay);

        obj.Init(_options, availableArray, r => {
            _choiceRoutine = r.Choice.ExecuteRoutine(player, positionInRegister);
            _hasMadeChoice = true;
        });

        yield return new WaitUntil(() => _hasMadeChoice);
        yield return _choiceRoutine;
    }
}