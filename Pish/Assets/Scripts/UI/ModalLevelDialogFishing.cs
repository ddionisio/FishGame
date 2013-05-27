using UnityEngine;
using System.Collections;

public class ModalLevelDialogFishing : ModalLevelDialogBase {
    public UISprite rankSprite;
    public UILabel rankLabel;
    
    protected override void OnInit(string level) {
        GameData.Info info = GameData.instance.GetInfo(level);
        float bestScore = info.bestScore;

        if(info != null) {
            if(rankSprite != null) {
                rankSprite.spriteName = info.GetRankSpriteRef(bestScore);
            }

            rankLabel.text = string.Format(info.rankLabelFormat, Mathf.RoundToInt(bestScore));
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
