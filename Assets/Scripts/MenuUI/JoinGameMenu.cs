using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinGameMenu : Menu {
    [SerializeField] TMP_InputField _codeInputField;
    
    public async void Submit() {
        // check if input is valid: must be 6 characters long and not null or whitespace
        if (string.IsNullOrWhiteSpace(_codeInputField.text) || _codeInputField.text.Length != 6) {
            Debug.LogWarning("Invalid code entered");
            return;
        }

        var success = await LobbySystem.Instance.JoinLobby(_codeInputField.text);

        if (success) {
            MenuSystem.Instance.PushMenu(MenuState.Room);
        } else {
            Debug.LogWarning("Failed to join lobby");
        }
    }
}