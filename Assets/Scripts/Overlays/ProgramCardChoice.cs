using System;
using UnityEngine;

public class ProgramCardChoice : Choice<ProgramCardData> {
    [SerializeField] Transform _choiceContainer;
    [SerializeField] ProgramCardChoiceItem _choicePrefab;

    public override int MaxOptions => 8;
    public override int MinOptions => 2;

    public void Init(ProgramCardData[] options, bool[] availableArray, Action<ChoiceResult> callback, bool isOptional = false) {
        Init(options, callback, isOptional);
        CreateCards(availableArray);
    }

    void CreateCards(bool[] availableArray) {
        for (int i = 0; i < Options.Length; i++) {
            var card = Instantiate(_choicePrefab, _choiceContainer);
            card.SetData(Options[i]);
            card.SetAvailable(availableArray[i]);
        }
    }

    void OnEnable() {
        ProgramCardChoiceItem.OnCardSelected += OnCardSelected;    
    }

    void OnDisable() {
        ProgramCardChoiceItem.OnCardSelected -= OnCardSelected;
    }

    void OnCardSelected(ProgramCardData card) {
        OnOptionChoose(card);
    }
}