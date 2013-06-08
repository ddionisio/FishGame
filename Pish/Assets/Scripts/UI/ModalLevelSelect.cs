using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {
    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.Menu, OnInput);
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.Menu, OnInput);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnInput(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("menu");
        }
    }
}
