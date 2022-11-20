using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgramCardChoice : Choice<ProgramCardData> {
    [SerializeField] Transform _choiceContainer;
    [SerializeField] ProgramCardChoiceItem _choicePrefab;

    protected override int MaxOptions => 8;
    protected override int MinOptions => 2;

    public void Init(ProgramCardData[] options, bool[] availableArray, Action<ChoiceResult> callback, bool isOptional = false) {
        Init(options, callback, isOptional);
        CreateCards(availableArray);
    }

    void CreateCards(IReadOnlyList<bool> availableArray) {
        for (var i = 0; i < Options.Length; i++) {
            var card = Instantiate(_choicePrefab, _choiceContainer);
            card.SetContent(Options[i]);
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