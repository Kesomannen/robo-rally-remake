using UnityEngine;
using UnityEngine.UI;

public class ExecutionRegister : Container<ProgramCardData>, ITooltipable {
    [Header("References")]
    [SerializeField] Image _artworkImage;

    bool _hidden;

    public string Header => _hidden ? "???" : Data.Header;
    public string Description => _hidden ? "???" : Data.Description;

    public bool Hidden {
        set {
            _hidden = value;
            _artworkImage.enabled = !_hidden;
        }
    }

    protected override void Serialize(ProgramCardData data) {
        _artworkImage.sprite = data.Artwork;
    }
}