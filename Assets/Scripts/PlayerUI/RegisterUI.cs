using UnityEngine;
using UnityEngine.EventSystems;

public class RegisterUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] Container<ProgramCardData> _cardContainer;
    [SerializeField] SoundEffect _onPlaceSound, _onRemoveSound;

    static Player Owner => PlayerManager.LocalPlayer;

    static readonly RegisterUI[] _registers = new RegisterUI[ExecutionPhase.RegisterCount];
    public static RegisterUI GetRegisterUI(int index) => _registers[index];
    
    public static bool Locked { get; set; }

    public bool IsEmpty => Owner.Program[_index] == null;

    void Awake() {
        _registers[_index] = this;
    }

    void OnEnable() {
        _cardContainer.gameObject.SetActive(false);
    }

    void Start() {
        Owner.Program.OnRegisterChanged += OnRegisterChanged;
    }
    
    void OnDestroy() {
        Owner.Program.OnRegisterChanged -= OnRegisterChanged;
    }

    void OnRegisterChanged(int index, ProgramCardData prev, ProgramCardData next) {
        if (!enabled) return;
        if (index != _index) return;

        if (next == null) {
            _cardContainer.gameObject.SetActive(false);
        } else {
            _cardContainer.SetContent(next);
            _cardContainer.gameObject.SetActive(true);
        }
    }


    public bool Place(Container<ProgramCardData> item) {
        if (Locked || !IsEmpty) return false;
        if (!item.Content.CanPlace(Owner, _index)) return false;

        _cardContainer.SetContent(item.Content);
        _cardContainer.gameObject.SetActive(true);
        Owner.Program.SetCard(_index, item.Content);
        
        _onPlaceSound.Play();
        
        return true;
    }

    public void Remove() {
        if (Locked || IsEmpty) return;
        if (!Owner.Hand.AddCard(_cardContainer.Content, CardPlacement.Top)) return;
        
        _cardContainer.gameObject.SetActive(false);
        Owner.Program.SetCard(_index, null);
        
        _onRemoveSound.Play();
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}