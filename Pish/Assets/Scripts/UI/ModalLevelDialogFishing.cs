using UnityEngine;
using System.Collections;

public class ModalLevelDialogFishing : ModalLevelDialogBase {
    public UISprite rankSprite;
    public UILabel rankLabel;
    
    protected override void OnInit(int level) {
        GameData.LevelScore bestScore = GameData.instance.GetLevelScore(level, true);

        if(rankLabel != null) {
            rankLabel.text = bestScore.text;
        }

        if(rankSprite != null) {
            rankSprite.spriteName = bestScore.medalSpriteRef;
            rankSprite.MakePixelPerfect();

            M8.NGUIExtUtil.LayoutRefresh(rankSprite.transform);
        }
    }

    protected override void OnPlay() {
    }

    void Awake() {
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
