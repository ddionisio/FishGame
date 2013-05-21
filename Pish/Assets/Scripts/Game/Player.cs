using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public float terrainImpactBatteryCost = 1.0f;

    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;

    public PlayerController controller {
        get { return mController; }
    }

    public PlayerStats stats {
        get { return mStats; }
    }

    void Awake() {
        mController = GetComponentInChildren<PlayerController>();
        mController.stateCallback += OnStateChange;
        mController.terrainHurtCallback += OnTerrainHurt;

        mStats = GetComponentInChildren<PlayerStats>();
        mStats.changeCallback += OnStatsChange;
    }

    // Use this for initialization
    void Start() {
        GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
        mCam = camGO.GetComponent<CameraController>();
        mCam.attachTo = controller.transform;
    }

    // Update is called once per frame
    void Update() {

    }

    void OnStatsChange(PlayerStats stats) {
        if(stats.curBattery == 0.0f) {
            Debug.Log("done");
        }
    }

    void OnStateChange(PlayerController pc, PlayerController.State prevState) {
        switch(pc.state) {
            case PlayerController.State.Stunned:
                //some sort of effect
                break;
        }
    }

    void OnTerrainHurt(PlayerController pc) {
        stats.curBattery -= terrainImpactBatteryCost;
    }
}
