using UnityEngine;

public class RoomMenu : Menu {
    protected override MenuState PreviousMenuState => MenuState.Main;

    public async override void Back() {
        await LobbySystem.Instance.LeaveLobby();
        base.Back();
    }
}