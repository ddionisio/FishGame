using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public const string lastLevelPlayedKey = "lastLevelPlayed";

    public enum CounterMode {
        None,
        Combo,
        Countdown,
    }

    public float comboDecayDelay;

    public float hurtTileMinRes;
    public float hurtTileDelay;

    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;
    private HUD mHUD;
    private float mScore;

    private CounterMode mCounterMode = CounterMode.None;

    private float mCurCounter = 1.0f;
    private bool mCounterProcessEnabled = false;
    private float mCurCounterTime = 0.0f;

    public float mCountdownMax = 60.0f;

    private M8.ImageEffects.Tile mTiler;

    public static string lastLevel {
        get {
            return UserData.instance.GetString(lastLevelPlayedKey);
        }
    }

    public float countdownMax {
        get { return mCountdownMax; }
        set {
            mCountdownMax = value;
            if(mCounterMode == CounterMode.Countdown)
                mHUD.RefreshCounterFill(mCurCounter / mCountdownMax);
        }
    }

    public CounterMode counterMode {
        get { return mCounterMode; }
        set {
            if(mCounterMode != value) {
                mCounterMode = value;

                switch(mCounterMode) {
                    case CounterMode.None:
                        mHUD.counterHolder.SetActive(false);
                        break;

                    case CounterMode.Combo:
                        mCurCounter = 1.0f;
                        mCurCounterTime = 0.0f;
                        mHUD.counterHolder.SetActive(false);
                        break;

                    case CounterMode.Countdown:
                        mCurCounter = mCountdownMax;
                        if(!mCounterProcessEnabled)
                            StartCoroutine(DoCountdown());
                        break;
                }
            }
        }
    }

    public HUD hud {
        get { return mHUD; }
    }

    public float currentCounter {
        get { return mCurCounter; }
        set {
            if(mCurCounter != value) {
                mCurCounter = value;

                switch(mCounterMode) {
                    case CounterMode.Combo:
                        mCurCounterTime = comboDecayDelay;

                        mHUD.counterHolder.SetActive(mCurCounter > 1.0f);
                        if(mCurCounter > 1.0f && !mCounterProcessEnabled)
                            StartCoroutine(DoCombo());
                        else
                            mHUD.RefreshCounter("x{0}", (int)mCurCounter);
                        break;

                    case CounterMode.Countdown:
                        if(!mCounterProcessEnabled)
                            StartCoroutine(DoCountdown());
                        else {
                            mHUD.RefreshCounter("{0}", (int)mCurCounter);
                            mHUD.RefreshCounterFill(mCurCounter / mCountdownMax);
                        }
                        break;
                }
            }
        }
    }

    public PlayerController controller {
        get { return mController; }
    }

    public float score {
        get { return mScore; }
        set { mScore = value; }
    }

    public PlayerStats stats {
        get { return mStats; }
    }

    public bool inputEnabled {
        get {
            return mController.inputEnabled;
        }

        set {
            bool setInput = mController.inputEnabled != value;
            mController.inputEnabled = value;

            if(setInput) {
                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(value) {
                        input.AddButtonCall(0, InputAction.Menu, OnInputPause);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Menu, OnInputPause);
                    }
                }
            }
        }
    }

    public void Stop() {
        inputEnabled = false;
        mController.state = PlayerController.State.None;
        mStats.Stop();
    }

    public override void SpawnFinish() {
        inputEnabled = true;
        mController.state = PlayerController.State.Normal;
        mStats.Run();
    }

    protected override void SpawnStart() {
        UserData.instance.SetString(lastLevelPlayedKey, Application.loadedLevelName);
    }

    protected override void OnDestroy() {
        inputEnabled = false;
        base.OnDestroy();
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
    }

    void OnInputPause(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("ingameOptions");
        }
    }

    void OnUIModalActive() {
        inputEnabled = false;
        Main.instance.sceneManager.Pause();
    }

    void OnUIModalInactive() {
        Main.instance.sceneManager.Resume();
        inputEnabled = true;
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
            if(mCounterMode == CounterMode.Combo) {
                mCurCounter = 1.0f;
                mCurCounterTime = 0.0f;
                mHUD.counterHolder.SetActive(false);
            }
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

                mScore += collect.fvalue * mCurCounter;

                mHUD.RefreshFishScore(mScore);

                if(mCounterMode == CounterMode.Combo) {
                    //update combo
                    mCurCounter += 1.0f;
                    mCurCounterTime = comboDecayDelay;

                    if(mCurCounter > 1.0f && !mCounterProcessEnabled)
                        StartCoroutine(DoCombo());
                    else
                        mHUD.RefreshCounter("x{0}", (int)mCurCounter);
                }
                break;

            case Collectible.Type.Energy:
                stats.curBattery += collect.fvalue;

                if(mController.jumpSpecial != null)
                    mController.jumpSpecial.SetCharge(mController, mController.jumpSpecial.curCharge + 1);

                break;

            case Collectible.Type.Collect:
                mCurCounter += collect.fvalue;

                mScore++;
                mHUD.RefreshFishScore(mScore);
                break;
        }
    }

    void OnCollect(Collectible collect) {
        // Debug.Log("collected: " + collect.type);
    }

    void OnJumpChargeChange(SpecialBase special) {
        mHUD.RefreshBoost(special);
    }

    IEnumerator DoCountdown() {
        mCounterProcessEnabled = true;

        mHUD.counterHolder.SetActive(true);

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while(mCurCounter > 0.0f) {
            yield return wait;

            mCurCounter -= Time.fixedDeltaTime;

            mHUD.RefreshCounterFill(mCurCounter / countdownMax);
            mHUD.RefreshCounter("{0}", (int)mCurCounter);
        }

        mHUD.counterHolder.SetActive(false);

        mCounterProcessEnabled = false;
    }

    IEnumerator DoCombo() {
        mCounterProcessEnabled = true;

        mHUD.counterHolder.SetActive(true);

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        mCurCounterTime = comboDecayDelay;

        mHUD.RefreshCounter("x{0}", (int)mCurCounter);

        while(mCurCounter > 1.0f) {
            while(mCurCounterTime > 0.0f) {
                mHUD.RefreshCounterFill(mCurCounterTime / comboDecayDelay);

                mCurCounterTime -= Time.fixedDeltaTime;

                yield return wait;
            }

            if(mCurCounter > 1.0f)
                mCurCounter -= 1.0f;

            mHUD.RefreshCounter("x{0}", (int)mCurCounter);
        }

        mHUD.counterHolder.SetActive(false);

        mCounterProcessEnabled = false;
    }
}
