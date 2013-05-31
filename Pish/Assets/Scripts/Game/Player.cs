using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public const string lastLevelPlayedKey = "lastLevelPlayed";

    public float comboDecayDelay;

    public float hurtTileMinRes;
    public float hurtTileDelay;
    
    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;
    private HUD mHUD;
    private float mScore;

    private float mCurCombo = 1.0f;
    private bool mComboDecayEnabled = false;
    private float mCurComboDecay = 0.0f;
    private WaitForFixedUpdate mComboWait;

    private M8.ImageEffects.Tile mTiler;

    public static string lastLevel {
        get {
            return UserData.instance.GetString(lastLevelPlayedKey);
        }
    }

    public float currentCombo {
        get { return mCurCombo; }
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
        mController.collectSensor.collector.collectQueueCallback += OnCollectQueue;

        mController.triggerEnterCallback += OnControllerTriggerEnter;

        mStats = GetComponentInChildren<PlayerStats>();
        mStats.changeCallback += OnStatsChange;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO.GetComponent<HUD>();

        GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
        mCam = camGO.GetComponent<CameraController>();
        mCam.attachTo = controller.transform;

        mTiler = mCam.lookCamera.GetComponent<M8.ImageEffects.Tile>();
        mTiler.enabled = false;

        activateOnStart = true;

        mComboWait = new WaitForFixedUpdate();
    }

    void OnStatsChange(PlayerStats stats) {
        mHUD.RefreshPlayerStats(stats);

        if(stats.curBattery == 0.0f) {
            Debug.Log("done");
        }
    }

    void OnStateChange(PlayerController pc, PlayerController.State prevState) {
        if(pc.state == PlayerController.State.Stunned) {
            //cancel combo
            mCurCombo = 1.0f;
            mCurComboDecay = 0.0f;
            mHUD.RefreshCombo(1);
        }
    }

    void OnHurt(PlayerController pc, float energy) {
        //quick screen distort
        if(!mTiler.enabled) {
            StartCoroutine(mTiler.DoTilePulse(hurtTileMinRes, hurtTileDelay));
        }

        stats.curBattery -= energy;
    }

    void OnControllerTriggerEnter(Collider c) {
        Collectible collect = c.GetComponent<Collectible>();
        if(collect != null) {
            OnCollectQueue(collect);
            OnCollect(collect);

            collect.Collected();
        }
    }

    void OnCollectQueue(Collectible collect) {
        Debug.Log("collecting: " + collect.type);

        switch(collect.type) {
            case Collectible.Type.Fish:
                FishInventory.Item newFish = new FishInventory.Item() { type = collect.svalue, ival = collect.ivalue, fval = collect.fvalue };
                FishInventory.instance.items.Add(newFish);

                mScore += collect.fvalue * mCurCombo;

                mHUD.RefreshFishScore(mScore);

                //update combo
                mCurCombo += 1.0f;
                mCurComboDecay = comboDecayDelay;

                if(mCurCombo > 1.0f && !mComboDecayEnabled)
                    StartCoroutine(DoCombo());
                else
                    mHUD.RefreshCombo((int)mCurCombo);

                break;

            case Collectible.Type.Energy:
                stats.curBattery += collect.fvalue;

                if(mController.jumpSpecial != null)
                    mController.jumpSpecial.SetCharge(mController, mController.jumpSpecial.curCharge + 1);

                break;
        }
    }

    void OnCollect(Collectible collect) {
       // Debug.Log("collected: " + collect.type);
    }

    void OnJumpChargeChange(SpecialBase special) {
        mHUD.RefreshBoost(special);
    }
    
    IEnumerator DoCombo() {
        mComboDecayEnabled = true;

        mCurComboDecay = comboDecayDelay;

        mHUD.RefreshCombo((int)mCurCombo);

        while(mCurCombo > 1.0f) {
            while(mCurComboDecay > 0.0f) {
                mHUD.RefreshComboFill(mCurComboDecay / comboDecayDelay);

                mCurComboDecay -= Time.fixedDeltaTime;

                yield return mComboWait;
            }

            if(mCurCombo > 1.0f)
                mCurCombo -= 1.0f;

            mHUD.RefreshCombo((int)mCurCombo);
        }

        mComboDecayEnabled = false;
    }
}
