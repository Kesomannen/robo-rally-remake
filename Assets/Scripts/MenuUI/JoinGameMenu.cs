using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class JoinGameMenu : Menu {
    [SerializeField] TMP_InputField _codeInputField;

    public async void Submit() { 
        if (string.IsNullOrWhiteSpace(_codeInputField.text) || _codeInputField.text.Length != 6) {
            Debug.LogWarning("Invalid code entered");
            return;
        }

        await LobbySystem.Instance.JoinLobby(_codeInputField.text);
        MenuSystem.Instance.PushMenu(MenuState.Room);
    }
}