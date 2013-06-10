using UnityEngine;
using System.Collections;

public class ModalLevelDialogBase : UIController {
    public string soundPlay;

    private int mLevel;

    public void Init(int level) {
        mLevel = level;

        OnInit(level);
    }

    protected virtual void OnInit(int level) {
    }

    protected virtual void OnPlay() {
    }


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
            if(!string.IsNullOrEmpty(soundPlay) && SoundPlayerGlobal.instance != null)
                SoundPlayerGlobal.instance.Play(soundPlay);

            OnPlay();

            Main.instance.sceneManager.LoadScene(GameData.instance.GetLevelName(mLevel));
        }
    }
}
