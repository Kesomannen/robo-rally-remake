using UnityEngine;
using UnityEngine.UI;

public class BoolChoice : Choice<bool> {
    [SerializeField] Button _yesButton;
    [SerializeField] Button _noButton;

    protected override void OnEnable() {
        base.OnEnable();
        _yesButton.onClick.AddListener(() => Toggle(0));
        _noButton.onClick.AddListener(() => Toggle(1));
    }

    protected override void OnInit() { }
}