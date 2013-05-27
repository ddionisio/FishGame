using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public const string lastLevelPlayedKey = "lastLevelPlayed";
    
    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;
    private HUD mHUD;
    private float mScore;

    public static string lastLevel {
        get {
            return UserData.instance.GetString(lastLevelPlayedKey);
        }
    }
        
    public PlayerController controller {
        get { return mController; }
    }

    public float score {
        get { return mScore; }
    }

    public PlayerStats stats {
        get { return mStats; }
    }

    public void Stop() {
        mController.inputEnabled = false;
        mController.state = PlayerController.State.None;
        mStats.Stop();
    }

    public override void SpawnFinish() {
        mController.inputEnabled = true;
        mController.state = PlayerController.State.Normal;
        mStats.Run();
    }

    protected override void SpawnStart() {
        UserData.instance.SetString(lastLevelPlayedKey, Application.loadedLevelName);
    }

    protected override void Awake() {
        base.Awake();

        mController = GetComponentInChildren<PlayerController>();
        mController.stateCallback += OnStateChange;
        mController.hurtCallback += OnHurt;
        mController.jumpSpecial.chargeChangeCallback += OnJumpChargeChange;
        mController.collectSensor.collector.collectReachedCallback += OnCollect;
        mController.triggerEnterCallback += OnControllerTriggerEnter;

        mStats = GetComponentInChildren<PlayerStats>();
        mStats.changeCallback += OnStatsChange;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO.GetComponent<HUD>();

        GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
        mCam = camGO.GetComponent<CameraController>();
        mCam.attachTo = controller.transform;

        activateOnStart = true;
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

    void OnControllerTriggerEnter(Collider c) {
        Collectible collect = c.GetComponent<Collectible>();
        if(collect != null) {
            OnCollect(collect);

            collect.Collected();
        }
    }

    void OnCollect(Collectible collect) {
        Debug.Log("collected: " + collect.type);

        switch(collect.type) {
            case Collectible.Type.Fish:
                FishInventory.Item newFish = new FishInventory.Item() { type = collect.svalue, ival = collect.ivalue, fval = collect.fvalue };
                FishInventory.instance.items.Add(newFish);

                mHUD.RefreshFishCount(FishInventory.instance.items.Count);

                mScore += collect.fvalue;
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
