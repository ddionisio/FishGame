using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;
    private HUD mHUD;

    public PlayerController controller {
        get { return mController; }
    }

    public PlayerStats stats {
        get { return mStats; }
    }

    void Awake() {
        mController = GetComponentInChildren<PlayerController>();
        mController.stateCallback += OnStateChange;
        mController.hurtCallback += OnHurt;
        mController.jumpSpecial.chargeChangeCallback += OnJumpChargeChange;
        mController.collectSensor.collector.collectReachedCallback += OnCollect;

        mStats = GetComponentInChildren<PlayerStats>();
        mStats.changeCallback += OnStatsChange;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO.GetComponent<HUD>();
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
        mHUD.RefreshPlayerStats(stats);

        if(stats.curBattery == 0.0f) {
            Debug.Log("done");
        }
    }

    void OnStateChange(PlayerController pc, PlayerController.State prevState) {
    }

    void OnHurt(PlayerController pc, float energy) {
        if(pc.state == PlayerController.State.Stunned) {
            //quick screen distort
        }

        stats.curBattery -= energy;
    }

    void OnCollect(Collectible collect) {
        Debug.Log("collected: " + collect.type);

        switch(collect.type) {
            case Collectible.Type.Fish:
                FishInventory.Item newFish = new FishInventory.Item() { type = collect.svalue, ival = collect.ivalue, fval = collect.fvalue };
                FishInventory.instance.items.Add(newFish);

                mHUD.RefreshFishCount(FishInventory.instance.items.Count);
                break;

            case Collectible.Type.Energy:
                stats.curBattery += collect.fvalue;
                break;
        }
    }

    void OnJumpChargeChange(SpecialBase special) {
        mHUD.RefreshBoost(special);
    }
}
