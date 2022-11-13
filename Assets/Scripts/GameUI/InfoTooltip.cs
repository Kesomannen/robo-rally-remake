using UnityEngine;

public class InfoTooltip : MonoBehaviour, ITooltipable {
    [SerializeField] string _header;
    [SerializeField] [TextArea] string _description;

    public string Header => _header;
    public string Description => _description;
}