using UnityEngine;
using System.Collections;

public class RescueResultController : BaseResultController {
    public UILabel statLabel;

    protected override void Start() {
        base.Start();

        if(statLabel != null) {
            //time taken
            float time = Player.lastLevelTime;
            int centi = Mathf.RoundToInt(time * 100.0f);
            int seconds = Mathf.RoundToInt(time);
            int minutes = seconds / 60;

            //treasure
            int treasureFound = Player.lastLevelTreasure;
            int treasureMax = Player.lastLevelTreasureMax;

            //deaths
            int numDeath = Player.lastLevelDeath;

            string format = statLabel.text;
            statLabel.text = string.Format(format, treasureFound, treasureMax, minutes % 60, seconds % 60, centi % 100, numDeath);
        }
    }
}
