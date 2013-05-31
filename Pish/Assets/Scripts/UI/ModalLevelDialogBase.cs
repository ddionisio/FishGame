using UnityEngine;
using System.Collections;

public abstract class ModalLevelDialogBase : UIController {
    private int mLevel;

    public void Init(int level) {
        mLevel = level;

        OnInit(level);
    }

    protected abstract void OnInit(int level);
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

            Main.instance.sceneManager.LoadScene(GameData.instance.GetLevelName(mLevel));
        }
    }
}
