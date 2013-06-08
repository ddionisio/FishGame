using UnityEngine;
using System.Collections;

public class LevelSelectController : MonoBehaviour {
    public const string lastLevelSelectKey = "lastLevelSelect";

    public UILabel scoring;

    public static string lastLevelSelect {
        get {
            return UserData.instance.GetString(lastLevelSelectKey);
        }
    }

    // Use this for initialization
    void Start() {
        UserData.instance.SetString(lastLevelSelectKey, Application.loadedLevelName);

        GameData.HIScore score = GameData.instance.GetHIScore();

        //check player ranking
        int curRank = UserData.instance.GetInt(Player.rankUserDataKey, 0);
        if(curRank < score.rankIndex) {
            UserData.instance.SetInt(Player.rankUserDataKey, score.rankIndex);
            StartCoroutine(NewRankDelay());
        }

        if(scoring != null) {
            string rankText, scoreText;

            GameData.instance.GetHIScoreString(score, out scoreText, out rankText);

            scoring.text = string.Format("{0}\n{1}", scoreText, rankText);
        }
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator NewRankDelay() {
        yield return new WaitForFixedUpdate();

        UIModalManager.instance.ModalOpen("newRank");
    }
}
