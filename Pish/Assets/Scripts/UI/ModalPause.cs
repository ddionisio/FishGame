using UnityEngine;
using System.Collections;

public class ModalPause : UIController {
    public UIEventListener help;
    public UIEventListener options;
    public UIEventListener restart;
    public UIEventListener exit;

    public string exitToScene; //leave blank to exit to lastLevelSelect

    protected override void OnActive(bool active) {
        if(active) {
            if(help != null)
                help.onClick += OnHelpClick;

            if(options != null)
                options.onClick += OnOptionsClick;

            if(restart != null)
                restart.onClick += OnRestartClick;

            if(exit != null)
                exit.onClick += OnExitClick;
        }
        else {
            if(help != null)
                help.onClick -= OnHelpClick;

            if(options != null)
                options.onClick -= OnOptionsClick;

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

    void OnHelpClick(GameObject go) {
        UIModalManager.instance.ModalOpen("help");
    }

    void OnOptionsClick(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }

    void OnRestartConfirm(bool yes) {
        if(yes)
            Main.instance.sceneManager.Reload();
    }

    void OnExitConfirm(bool yes) {
        if(yes) {
            if(!string.IsNullOrEmpty(exitToScene))
                Main.instance.sceneManager.LoadScene(exitToScene);
            else
                Main.instance.sceneManager.LoadScene(LevelSelectController.lastLevelSelect);
        }
    }
}