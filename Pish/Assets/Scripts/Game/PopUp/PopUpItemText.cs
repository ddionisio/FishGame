using UnityEngine;
using System.Collections;

public class PopUpItemText : PopUpItemBase {
    public tk2dTextMesh textSprite;

    public override void Init(string text, Vector2 dir, float speed, float delay) {
        base.Init(text, dir, speed, delay);

        textSprite.text = text;
        textSprite.Commit();
    }
}
