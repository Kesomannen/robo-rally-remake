using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateGameMenu : Menu {
    [SerializeField] Toggle _privateCheckbox;
    [SerializeField] TMP_InputField _nameInputField;

    protected override MenuState PreviousMenuState => MenuState.Main;

    public async void Submit() {
        if (string.IsNullOrWhiteSpace(_nameInputField.text)) {
            Debug.LogWarning("No name entered");
            return;
        }

        var lobbyData = new LobbyData {
            Name = _nameInputField.text,
            MapID = 0,
            MaxPlayers = 4,
            IsPrivate = _privateCheckbox.isOn,
        };

        var success = await LobbySystem.Instance.CreateLobby(lobbyData);

        if (success) {
            MenuSystem.Instance.ChangeMenu(MenuState.Room);
        } else {
            Debug.LogWarning("Failed to create lobby");
        }
    }
}