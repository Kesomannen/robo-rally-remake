using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinGameMenu : Menu {
    [SerializeField] TMP_InputField _codeInputField;
    
    protected override MenuState PreviousMenuState => MenuState.Main;
}