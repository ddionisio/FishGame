using UnityEngine;
using System.Collections;

public class ModalLevelDialogRanked : ModalLevelDialogBase {
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
        }
    }

    protected override void OnOpen() {
        if(rankSprite != null && rankSprite.transform.parent != null) {
            NGUILayoutBase.RefreshNow(rankSprite.transform.parent);
        }
    }

    protected override void OnPlay() {
    }
}
