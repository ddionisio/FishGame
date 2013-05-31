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

        if(scoring != null) {
            string rank, score;

            GameData.instance.GetHIScoreString(out score, out rank);

            scoring.text = string.Format("{0}\n{1}", score, rank);
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
