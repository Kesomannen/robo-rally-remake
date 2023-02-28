using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinGameMenu : Menu {
    [SerializeField] TMP_InputField _codeInputField;
    [SerializeField] Selectable _submitButton;

    void OnEnable() {
        _codeInputField.text = "";
        _submitButton.interactable = false;
        _codeInputField.onValueChanged.AddListener(OnCodeChanged);

        var clipboard = GUIUtility.systemCopyBuffer;
        if (clipboard.Length == 6) {
            _codeInputField.text = clipboard;
        }
    }
    
    void OnDisable() {
        _codeInputField.onValueChanged.RemoveListener(OnCodeChanged);
    }
    
    void OnCodeChanged(string input) {
        var code = input;
        if (code.Length > 6) {
            code = code[..6];
        }
        code = code.Replace(" ", "");
        code = code.ToUpper();
        _codeInputField.text = code;
        
        _submitButton.interactable = code.Length == 6;
    }

    public async void Submit() { 
        if (string.IsNullOrWhiteSpace(_codeInputField.text) || _codeInputField.text.Length != 6) {
            Debug.LogWarning("Invalid code entered");
            return;
        }

        bool successful;
        using (new LoadingScreen($"Joining lobby {_codeInputField.text}")) {
            successful = await LobbySystem.Instance.JoinLobby(_codeInputField.text);
        }

        if (successful) {
            MenuSystem.Instance.PushMenu(MenuState.Room);   
        } else {
            MenuUtils.Instance.ShowError("Failed to join lobby");
        }
    }
}