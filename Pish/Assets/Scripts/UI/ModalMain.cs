using UnityEngine;
using System.Collections;

public class ModalMain : UIController {
    public UIEventListener continueGame;
    public UIEventListener newGame;
    public UIEventListener options;
    public UIEventListener credits;

    protected override void OnActive(bool active) {
        if(active) {
        }
        else {
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
        if(continueGame != null) {
            continueGame.onClick += OnContinue;
        }

        newGame.onClick += OnNew;
        options.onClick += OnOptions;
        credits.onClick += OnCredits;
    }

    void OnContinue(GameObject go) {
    }

    void OnNew(GameObject go) {
        //create profile
        //stuff
        Main.instance.sceneManager.LoadScene("intro");
    }

    void OnOptions(GameObject go) {
    }

    void OnCredits(GameObject go) {
    }
}
