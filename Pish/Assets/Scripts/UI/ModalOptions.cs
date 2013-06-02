using UnityEngine;
using System.Collections;

public class ModalOptions : UIController {
    public UIEventListener restart;
    public UIEventListener exit;

    protected override void OnActive(bool active) {
        if(active) {
            if(restart != null)
                restart.onClick += OnRestartClick;

            if(exit != null)
                exit.onClick += OnExitClick;
        }
        else {
            if(restart != null)
                restart.onClick -= OnRestartClick;

            if(exit != null)
                exit.onClick -= OnExitClick;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnRestartClick(GameObject go) {
        UIModalConfirm.Open("RESTART", null, OnRestartConfirm);
    }

    void OnExitClick(GameObject go) {
        UIModalConfirm.Open("EXIT", null, OnExitConfirm);
    }

    void OnRestartConfirm(bool yes) {
        if(yes)
            Main.instance.sceneManager.Reload();
    }

    void OnExitConfirm(bool yes) {
        if(yes)
            Main.instance.sceneManager.LoadScene(LevelSelectController.lastLevelSelect);
    }
}