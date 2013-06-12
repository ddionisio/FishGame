using UnityEngine;
using System.Collections;

public class ModalNewRank : UIController {
    public UILabel rankLabel;

    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.MenuAccept, OnInput);

            //update rank label
            GameData.HIScore hiscore = GameData.instance.GetHIScore();

            rankLabel.text = hiscore.rank;
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuAccept, OnInput);
        }
    }

    protected override void OnOpen() {
        NGUILayoutBase.RefreshNow(transform);
    }

    protected override void OnClose() {
    }

    void OnInput(InputManager.Info dat) {
        if(dat.state == InputManager.State.Released)
            UIModalManager.instance.ModalReplace("levelSelect");
    }
}
