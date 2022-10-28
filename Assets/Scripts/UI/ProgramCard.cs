using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgramCard : MonoBehaviour {
    [SerializeField] Image _artwork;
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _description;

    public ProgramCardData Data { get; private set; }

    public virtual void SetData(ProgramCardData data) {
        Data = data;
        _artwork.sprite = data.Artwork;
        _name.text = data.Name;
        _description.text = data.Description;
    }
}