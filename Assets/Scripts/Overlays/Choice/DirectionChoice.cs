using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionChoice : Choice<Vector2Int> {
    [SerializeField] Button _upButton;
    [SerializeField] Button _downButton;
    [SerializeField] Button _leftButton;
    [SerializeField] Button _rightButton;
    [SerializeField] Button _zeroButton;

    int ChoiceIndex(Vector2Int direction) => Options.IndexOf(direction);

    public static readonly IReadOnlyList<Vector2Int> DirectionsWithoutZero = new[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    protected override void OnInit() {
        Configure(_upButton, Vector2Int.up);
        Configure(_downButton, Vector2Int.down);
        Configure(_leftButton, Vector2Int.left);
        Configure(_rightButton, Vector2Int.right);
        Configure(_zeroButton, Vector2Int.zero);

        void Configure(Button button, Vector2Int direction) {
            var index = ChoiceIndex(direction);
            if (index == -1) {
                button.gameObject.SetActive(false);
                return;
            }
            button.onClick.AddListener(() => Toggle(index));
            button.interactable = AvailablePredicate(direction);
        }
    }
}