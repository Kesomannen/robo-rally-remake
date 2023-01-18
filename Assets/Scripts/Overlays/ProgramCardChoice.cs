using UnityEngine;

public class ProgramCardChoice : Choice<ProgramCardData> {
    [Header("References")]
    [SerializeField] Transform _choiceContainer;
    
    [Header("Prefabs")]
    [SerializeField] ProgramCardChoiceItem _choicePrefab;

    protected override void OnInit() {
        CreateCards();
    }
    void CreateCards() {
        for (var i = 0; i < Options.Count; i++) {
            var card = Instantiate(_choicePrefab, _choiceContainer);
            card.SetContent(Options[i]);
            card.SetAvailable(AvailableOptions[i]);
        }
    }
    
    protected override void OnEnable() {
        ProgramCardChoiceItem.OnCardSelected += OnCardSelected;    
    }

    protected override void OnDisable() {
        ProgramCardChoiceItem.OnCardSelected -= OnCardSelected;
    }

    void OnCardSelected(ProgramCardData card) {
        OnOptionChoose(card);
    }
}