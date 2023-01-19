using UnityEngine;

public class ContainerChoice<T> : Choice<T> {
    [Header("References")]
    [SerializeField] Transform _choiceContainer;
    
    [Header("Prefabs")]
    [SerializeField] ChoiceItem<T> _choicePrefab;

    protected override void OnInit() {
        CreateContainers();
    }
    
    void CreateContainers() {
        for (var i = 0; i < Options.Count; i++) {
            var card = Instantiate(_choicePrefab, _choiceContainer);
            card.Container.SetContent(Options[i]);
            card.SetAvailable(AvailableOptions[i]);
        }
    }
    
    protected override void OnEnable() {
        ChoiceItem<T>.OnCardSelected += OnCardSelected;    
    }

    protected override void OnDisable() {
        ChoiceItem<T>.OnCardSelected -= OnCardSelected;
    }

    void OnCardSelected(T card) {
        OnOptionChoose(card);
    }
}