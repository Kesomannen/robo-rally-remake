using UnityEngine;

public abstract class Menu : MonoBehaviour {
    protected abstract MenuState PreviousMenuState { get; }

    public virtual void Hide() {
        gameObject.SetActive(false);
    }

    public virtual void Show() {
        gameObject.SetActive(true);
    }

    public virtual void Back() {
        MenuSystem.Instance.ChangeMenu(PreviousMenuState);
    }
}