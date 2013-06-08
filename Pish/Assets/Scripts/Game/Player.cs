using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : EntityBase {
    public const string levelTimeSceneValueKey = "lvlTime";
    public const string levelRescueSceneValueKey = "lvlRescue";
    public const string levelTreasureSceneValueKey = "lvlTreasure";
    public const string levelTreasureMaxSceneValueKey = "lvlTreasureMax";
    public const string levelDeathSceneValueKey = "lvlDeath";

    public const string lastLevelPlayedKey = "lastLevelPlayed";

    public const string rankUserDataKey = "rank";

    public delegate void Callback(Player p);
    
    public enum CounterMode {
        None,
        Combo,
        Countdown,
    }

    public float comboDecayDelay;

    public float hurtTileMinRes;
    public float hurtTileDelay;

    public bool storeCollectibles = false; //set this to true when there are checkpoints and we are using respawn

    public float warpRadius;
    public float warpAngleMax;
    public float warpDelay;

    public event Callback warpDoneCallback;

    private PlayerController mController;
    private PlayerStats mStats;
    private CameraController mCam;
    private HUD mHUD;
    private float mScore;

    private CounterMode mCounterMode = CounterMode.None;

    private float mCurCounter = 1.0f;
    private bool mCounterProcessEnabled = false;
    private float mCurCounterTime = 0.0f;

    private float mCountdownMax = 60.0f;

    private int mRescueCount = 0;
    private int mTreasureCount = 0;
    private int mNumDeath = 0;

    private M8.ImageEffects.Tile mTiler;

    private Vector2 mLastSpawnPos;
    private Checkpoint mLastCheckpoint;

    private List<CollectibleEntity> mStoredCollectibles;
    private float mStoredScore=0;
    private int mStoredTreasureCount=0;
    private int mStoredRescueCount=0;

    private VortexEffect mVortex;

    public static string lastLevel {
        get {
            return UserData.instance.GetString(lastLevelPlayedKey);
        }
    }

    public static float lastLevelTime {
        get {
            return SceneState.instance.GetGlobalValueFloat(levelTimeSceneValueKey);
        }
    }

    public static int lastLevelRescue {
        get {
            return SceneState.instance.GetGlobalValue(levelRescueSceneValueKey);
        }
    }

    public static int lastLevelTreasure {
        get {
            return SceneState.instance.GetGlobalValue(levelTreasureSceneValueKey);
        }
    }

    public static int lastLevelTreasureMax {
        get {
            return SceneState.instance.GetGlobalValue(levelTreasureMaxSceneValueKey);
        }
    }

    public static int lastLevelDeath {
        get {
            return SceneState.instance.GetGlobalValue(levelDeathSceneValueKey);
        }
    }

    public int rescueCount {
        get { return mRescueCount; }
    }

    public int treasureCount {
        get { return mTreasureCount; }
    }

    public int numDeath {
        get { return mNumDeath; }
    }

    public float countdownMax {
        get { return mCountdownMax; }
        set {
            mCountdownMax = value;
            if(mCounterMode == CounterMode.Countdown)
                mHUD.RefreshCounterFill(mCurCounter / mCountdownMax);
        }
    }

    public Vector3 lastSpawnPosition {
        get { return mLastSpawnPos; }
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

    /// <summary>
    /// Relocate back to previous spawn and reset battery
    /// </summary>
    public void Respawn() {
        mController.state = PlayerController.State.Normal;
        mController.curVelocity = Vector2.zero;
        mController.transform.position = mLastSpawnPos;
        mStats.ResetStats();

        if(storeCollectibles) {
            Debug.Log("respawning, stored count: " + mStoredCollectibles.Count);

            foreach(CollectibleEntity collectEntity in mStoredCollectibles) {
                collectEntity.gameObject.SetActive(true);
                collectEntity.collectible.collectFlagged = false;
            }

            mScore = mStoredScore;
            mTreasureCount = mStoredTreasureCount;
            mRescueCount = mStoredRescueCount;
                        
            Debug.Log("treasureCount: " + mTreasureCount);
            Debug.Log("rescueCount: " + mRescueCount);

            mStoredCollectibles.Clear();

            mHUD.RescueRefresh(mRescueCount);
            mHUD.RefreshFishScore(mScore);
        }
    }

    public void Stop() {
        inputEnabled = false;
        mController.state = PlayerController.State.None;
        mStats.Stop();

        //save some data for use outside game
        SceneState ss = SceneState.instance;
        if(ss != null) {
            ss.SetGlobalValueFloat(levelTimeSceneValueKey, mHUD.timerCurrent, false);
            ss.SetGlobalValue(levelRescueSceneValueKey, mRescueCount, false);
            ss.SetGlobalValue(levelTreasureSceneValueKey, mTreasureCount, false);
            ss.SetGlobalValue(levelDeathSceneValueKey, mNumDeath, false);
        }

        //save score
        GameData.instance.SaveLevelScore(Application.loadedLevelName, this);
    }

    public void Warp(bool includeCollector, bool isOut) {
        StartCoroutine(DoWarp(includeCollector, isOut));
    }

    public override void SpawnFinish() {
        inputEnabled = true;
        mController.state = PlayerController.State.Normal;
        mStats.Run();
    }

    protected override void SpawnStart() {
        UserData.instance.SetString(lastLevelPlayedKey, Application.loadedLevelName);

        mScore = 0.0f;
        mCounterProcessEnabled = false;
        mNumDeath = 0;
        mRescueCount = 0;
        mTreasureCount = 0;

        mStoredScore=0;
        mStoredTreasureCount=0;
        mStoredRescueCount=0;

        mLastSpawnPos = mController.transform.position;
    }

    protected override void OnDestroy() {
        inputEnabled = false;

        warpDoneCallback = null;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        if(storeCollectibles)
            mStoredCollectibles = new List<CollectibleEntity>();

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

        mVortex = mCam.lookCamera.GetComponent<VortexEffect>();
        if(mVortex != null)
            mVortex.enabled = false;

        mTiler = mCam.lookCamera.GetComponent<M8.ImageEffects.Tile>();
        mTiler.enabled = false;

        activateOnStart = true;
    }

    void OnInputPause(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("ingameOptions");
            Main.instance.sceneManager.Pause();
        }
    }

    void OnUIModalActive() {
        inputEnabled = false;
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

        float prevBattery = stats.curBattery;

        stats.curBattery -= energy;

        if(stats.curBattery != prevBattery && stats.curBattery == 0.0f)
            mNumDeath++;
    }

    void OnControllerTriggerEnter(Collider c) {
        //checkpoint?
        Checkpoint checkpoint = c.GetComponent<Checkpoint>();
        if(checkpoint != null) {
            if(mLastCheckpoint != checkpoint) {
                if(mLastCheckpoint != null)
                    mLastCheckpoint.SetOpen(false);

                mLastCheckpoint = checkpoint;
                mLastCheckpoint.SetOpen(true);

                mLastSpawnPos = mLastCheckpoint.spawnPoint.position;
            }

            checkpoint.Triggered();

            //restore battery and energy
            if(mStats.curBattery < mStats.batteryStart)
                mStats.curBattery = mStats.batteryStart;

            if(mController.jumpSpecial != null)
                mController.jumpSpecial.SetCharge(mController, mController.jumpSpecial.maxCharge);

            //clear out stored collectibles
            if(storeCollectibles) {
                foreach(CollectibleEntity collectEntity in mStoredCollectibles) {
                    collectEntity.Release();
                }

                mStoredCollectibles.Clear();
            }

            //save level data
            mStoredScore = mScore;
            mStoredTreasureCount = mTreasureCount;
            mStoredRescueCount = mRescueCount;
        }
        else {
            //collect?
            Collectible collect = c.GetComponent<Collectible>();
            if(collect != null) {
                switch(collect.type) {
                    case Collectible.Type.Energy:
                    case Collectible.Type.Collect:
                        //collect immediately
                        OnCollectQueue(collect);
                        OnCollect(collect);

                        collect.Collected();
                        break;

                    case Collectible.Type.Rescue:
                        //queue to collect
                        mController.collectSensor.collector.AddToQueue(collect);
                        break;

                    case Collectible.Type.Treasure:
                        if(mController.collectSensor.collector.gameObject.activeInHierarchy) {
                            mController.collectSensor.collector.AddToQueue(collect);
                        }
                        else {
                            //collect immediately
                            OnCollectQueue(collect);
                            OnCollect(collect);
                            collect.Collected();
                        }
                        break;
                }
            }
        }
    }

    void OnCollectQueue(Collectible collect) {
        Debug.Log("collecting: " + collect.type);

        switch(collect.type) {
            case Collectible.Type.Fish:
                FishInventory.Item newFish = new FishInventory.Item() { type = collect.svalue, ival = collect.ivalue, fval = collect.fvalue };
                FishInventory.instance.items.Add(newFish);

                ScoreUpdate(collect.fvalue * mCurCounter);

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
                PopUp.instance.SpawnIcon(mController.transform.position, PopUp.Icon.Battery);

                if(mController.jumpSpecial != null)
                    mController.jumpSpecial.SetCharge(mController, mController.jumpSpecial.curCharge + 1);

                break;

            case Collectible.Type.Collect:
                mCurCounter += collect.fvalue;

                stats.curBattery += collect.fvalue*2.0f;
                PopUp.instance.SpawnIcon(mController.transform.position, PopUp.Icon.Battery);

                ScoreUpdate(1.0f);
                break;

            case Collectible.Type.Rescue:
                mRescueCount++;
                mHUD.RescueRefresh(mRescueCount);

                Debug.Log("rescueCount: " + mRescueCount);

                StoreCollect(collect);
                break;

            case Collectible.Type.Treasure:
                mTreasureCount++;

                ScoreUpdate(collect.fvalue);
                
                Debug.Log("treasureCount: " + mTreasureCount);

                StoreCollect(collect);
                break;
        }
    }

    void ScoreUpdate(float delta) {
        mScore += delta;

        mHUD.RefreshFishScore(mScore);

        PopUp.instance.SpawnText(mController.transform.position, "+" + delta);
    }

    void StoreCollect(Collectible collect) {
        if(storeCollectibles) {
            CollectibleEntity ce = collect.GetComponent<CollectibleEntity>();
            ce.deactivateOnCollect = true;
            mStoredCollectibles.Add(ce);

            Debug.Log("stored count: " + mStoredCollectibles.Count);
        }
    }

    void OnCollect(Collectible collect) {
        // Debug.Log("collected: " + collect.type);
    }

    void OnJumpChargeChange(SpecialBase special) {
        mHUD.RefreshBoost(special);
    }

    IEnumerator DoWarp(bool includeCollector, bool isOut) {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        //yield return wait;

        if(mVortex != null) {
            Vector3 pPos = mController.transform.position;

            mCam.transform.position = pPos;
            mCam.attachTo = null;

            if(!isOut) {
                mController.gameObject.SetActive(false);

                if(includeCollector)
                    mController.collectSensor.collector.gameObject.SetActive(false);
            }

            mVortex.enabled = true;
            mVortex.radius = new Vector2(warpRadius, warpRadius);
            mVortex.angle = 0.0f;
            mVortex.center = mCam.lookCamera.WorldToViewportPoint(pPos);

            bool entered = false;

            float time = 0.0f;
            while(time < warpDelay) {
                if(time >= warpDelay*0.5f && !entered) {
                    if(isOut) {
                        mController.gameObject.SetActive(false);

                        if(includeCollector)
                            mController.collectSensor.collector.gameObject.SetActive(false);
                    }
                    else {
                        mController.gameObject.SetActive(true);
                    }

                    entered = true;
                }

                float t = Mathf.Sin(Mathf.PI*(time/warpDelay));
                t *= t;

                mVortex.angle = warpAngleMax * t;

                time += Time.fixedDeltaTime;
                yield return wait;
            }

            mVortex.enabled = false;

            if(!isOut) {
                if(includeCollector) {
                    pPos = mController.transform.position;
                    Vector3 cPos = mController.collectSensor.collector.transform.position;
                    mController.collectSensor.collector.transform.position = new Vector3(pPos.x, pPos.y, cPos.z);
                    mController.collectSensor.collector.gameObject.SetActive(true);
                }
            }

            mCam.attachTo = mController.transform;
        }

        if(warpDoneCallback != null)
            warpDoneCallback(this);

        yield break;
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
