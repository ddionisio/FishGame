using UnityEngine;
using System.Collections;

public class ModalMenu : UIController {
    public UIEventListener help;
    public UIEventListener options;
    public UIEventListener exit;

    public string exitToScene;

    protected override void OnActive(bool active) {
        if(active) {
            if(help != null)
                help.onClick += OnHelpClick;

            if(options != null)
                options.onClick += OnOptionsClick;

            if(exit != null)
                exit.onClick += OnExitClick;
        }
        else {
            if(help != null)
                help.onClick -= OnHelpClick;

            if(options != null)
                options.onClick -= OnOptionsClick;

            if(exit != null)
                exit.onClick -= OnExitClick;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
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

    void OnExitConfirm(bool yes) {
        if(yes) {
            if(!string.IsNullOrEmpty(exitToScene))
                Main.instance.sceneManager.LoadScene(exitToScene);
            else
                UIModalManager.instance.ModalCloseTop();
        }
    }
}