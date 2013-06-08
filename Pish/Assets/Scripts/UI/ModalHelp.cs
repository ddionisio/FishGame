using UnityEngine;
using System.Collections;

public class ModalHelp : UIController {
    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.MenuAccept, OnInput);
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuAccept, OnInput);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnInput(InputManager.Info dat) {
        if(dat.state == InputManager.State.Released)
            UIModalManager.instance.ModalCloseTop();
    }
}
