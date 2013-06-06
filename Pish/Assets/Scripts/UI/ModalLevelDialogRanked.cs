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

            M8.NGUIExtUtil.LayoutRefresh(rankSprite.transform);
        }
    }

    protected override void OnPlay() {
    }
}
