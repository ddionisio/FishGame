using UnityEngine;
using System.Collections;

public class Fish : EntityBase {
    public const int StateNormal = 1;
    public const int StateStunned = 2;

    public GameObject reticle;

    public bool targetable = true;

    private FishStats mStats;
    private FishController mController;
    private Collectible mCollectible;
    private NGUIPointAt mPointIndicator;

    private HUD mHUD;

    public FishStats stats {
        get { return mStats; }
    }

    public FishController controller {
        get { return mController; }
    }
    
    public bool reticleEnabled {
        get {
            return reticle != null && reticle.activeSelf;
        }

        set {
            if(reticle != null) {
                reticle.SetActive(value);
            }
        }
    }

    public float PlayerContact(PlayerController pc, Vector2 dir, float speed, ControllerColliderHit hit) {
        float pushBackSpeed;
        float pushSpeed;

        if(!mController.playerHitInvulnerable && speed >= pc.fishHitSpeedCriteria) {
            Debug.Log("fish hurt");

            stats.curHit--;

            pushSpeed = pc.fishHitPushSpeed;
            pushBackSpeed = 0.0f;
        }
        else {          
            //stun?
            if(mController.playerContactStun) {
                pc.state = PlayerController.State.Stunned;
            }

            //see if we can hurt the player
            if(mController.playerContactEnergy != 0.0f) {
                pc.Hurt(mController.playerContactEnergy);
            }

            pushSpeed = pc.fishContactSpeed;
            pushBackSpeed = mController.playerContactPushSpeed;
        }

        rigidbody.velocity = hit.moveDirection * pushSpeed;

        return pushBackSpeed;
    }

    protected override void StateChanged() {
        //prev
        
        //new
        switch(state) {
            case StateNormal:
                break;

            case StateStunned:
                gameObject.layer = Layers.collect;
                mController.curMoveMode = FishController.MoveMode.Fall;
                break;

            case StateInvalid:
                mController.curMoveMode = FishController.MoveMode.NumModes;
                break;
        }
    }

    public override void SpawnFinish() {
        reticleEnabled = false;
        state = StateNormal;
    }

    protected override void SpawnStart() {
        SpawnFinish();
    }

    protected override void OnDespawned() {
        if(mPointIndicator != null) {
            mHUD.ReleasePointer(mPointIndicator);
            mPointIndicator = null;
        }

        state = StateInvalid;
        reticleEnabled = false;
        gameObject.layer = Layers.fish;

        mController.curMoveMode = FishController.MoveMode.NumModes;
        mController.flockUnit.ResetData();

        mCollectible.collectFlagged = false;

        base.OnDespawned();
    }
    
    protected override void Awake() {
        base.Awake();

        mStats = GetComponent<FishStats>();
        mStats.changeCallback += OnStatChange;

        mController = GetComponent<FishController>();

        mCollectible = GetComponent<Collectible>();
        mCollectible.collectedCallback += Release;
        mCollectible.svalue = spawnType;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO.GetComponent<HUD>();
    }

    void LateUpdate() {
        if(targetable && mPointIndicator == null) {
            mPointIndicator = mHUD.AllocatePointer();
            mPointIndicator.SetPOI(transform);
        }
    }
    
    void OnStatChange(FishStats stats) {
        if(stats.curHit == 0) {
            state = StateStunned;
        }
    }
}
