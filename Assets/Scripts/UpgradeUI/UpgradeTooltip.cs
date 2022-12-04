using TMPro;
using UnityEngine;

public class UpgradeTooltip : MonoBehaviour {
    [SerializeField] TMP_Text _headerText, _descriptionText;

    public void SetContent(string header, string description) {
        _headerText.text = header;
        _descriptionText.text = description;
    }
}