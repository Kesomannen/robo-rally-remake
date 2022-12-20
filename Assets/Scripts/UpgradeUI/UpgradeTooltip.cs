using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTooltip : MonoBehaviour {
    [SerializeField] TMP_Text _headerText, _descriptionText;
    [SerializeField] LayoutElement _layoutElement;
    [SerializeField] int _characterWrapLimit;

    public void SetContent(string header, string description) {
        _headerText.text = header;
        _descriptionText.text = description;
        _layoutElement.enabled = description.Length > _characterWrapLimit;
    }
}