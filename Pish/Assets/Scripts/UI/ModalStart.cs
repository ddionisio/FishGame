using UnityEngine;
using System.Collections;

public class ModalStart : UIController {

    protected override void OnActive(bool active) {
        InputManager input = Main.instance.input;

        if(active) {
            input.AddButtonCall(0, InputAction.MenuAccept, OnPress);
        }
        else {
            input.RemoveButtonCall(0, InputAction.MenuAccept, OnPress);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnPress(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            //determine if we have an existing save
            UIModalManager.instance.ModalOpen("mainNew");
        }
    }
}
