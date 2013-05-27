using UnityEngine;
using System.Collections;

public class LevelSelectController : MonoBehaviour {
    public const string lastLevelSelectKey = "lastLevelSelect";

    public static string lastLevelSelect {
        get {
            return UserData.instance.GetString(lastLevelSelectKey);
        }
    }

    // Use this for initialization
    void Start() {
        UserData.instance.SetString(lastLevelSelectKey, Application.loadedLevelName);
    }

    // Update is called once per frame
    void Update() {

    }
}
