using UnityEngine;
using System.Collections;

public abstract class ModalLevelDialogBase : UIController {
    private string mLevel;

    public void Init(string level) {
        mLevel = level;

        OnInit(level);
    }

    protected abstract void OnInit(string data);
    protected abstract void OnPlay();


    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.MenuAccept, OnInputPlay);
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuAccept, OnInputPlay);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }


    void OnInputPlay(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            OnPlay();

            Main.instance.sceneManager.LoadScene(mLevel);
        }
    }
}
