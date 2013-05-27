using UnityEngine;
using System.Collections;

public class ModalIntro : UIController {
    public string toScene;

    protected override void OnActive(bool active) {
        InputManager input = Main.instance.input;

        if(active) {
            input.AddButtonCall(0, InputAction.Hook, OnPress);
        }
        else {
            input.RemoveButtonCall(0, InputAction.Hook, OnPress);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnPress(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            Main.instance.sceneManager.LoadScene(toScene);
        }
    }
}
