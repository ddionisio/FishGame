using UnityEngine;
using System.Collections;

public class BaseResultController : MonoBehaviour {
    public UILabel resultLabel;
    public UISprite resultSprite;

    public UILabel resultBestLabel;
    public UISprite resultBestSprite;

    protected virtual void OnDestroy() {
        if(Main.instance != null && Main.instance.input != null)
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuAccept, OnInputContinue);
    }

    protected virtual void Start() {
        Main.instance.input.AddButtonCall(0, InputAction.MenuAccept, OnInputContinue);

        //
        string level = Player.lastLevel;

        GameData.LevelScore bestScore = GameData.instance.GetLevelScore(level, true);
        GameData.LevelScore score = GameData.instance.GetLevelScore(level, false);

        resultLabel.text = score.text;
        resultSprite.spriteName = score.medalSpriteRef;
        resultSprite.MakePixelPerfect();

        resultBestLabel.text = bestScore.text;
        resultBestSprite.spriteName = bestScore.medalSpriteRef;
        resultBestSprite.MakePixelPerfect();
    }

    void OnInputContinue(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            Main.instance.sceneManager.LoadScene(LevelSelectController.lastLevelSelect);
        }
    }
}