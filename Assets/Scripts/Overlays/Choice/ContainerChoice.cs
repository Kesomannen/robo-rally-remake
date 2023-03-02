using UnityEngine;
using UnityEngine.Serialization;

public class ContainerChoice<T> : Choice<T> {
    [FormerlySerializedAs("_choiceContainer")]
    [SerializeField] Transform _choiceParent;
    [SerializeField] ChoiceItem<T> _choicePrefab;

    protected override void OnInit() {
        for (var i = 0; i < Options.Count; i++) {
            var option = Options[i];
            
            var item = Instantiate(_choicePrefab, _choiceParent);
            item.Container.SetContent(option);
            item.SetAvailable(AvailablePredicate(option));
            item.OptionIndex = i;
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        ChoiceItem<T>.OnItemClicked += OnItemClicked;    
    }

    protected override void OnDisable() {
        base.OnDisable();
        ChoiceItem<T>.OnItemClicked -= OnItemClicked;
    }

    void OnItemClicked(int option) {
        Toggle(option);
    }
}