using UnityEngine;
using System.Collections;

public class Fish : EntityBase {
    public const int StateNormal = 1;
    public const int StateStunned = 2;
        
    public bool invulnerable = false;
    public float pushBackSpeed = 4.0f; //the push back speed when player contacts us.
    public float pushForwardSpeed = 0.0f; //the push returned to player
    public bool contactStun = false; //stun player
    public float contactReduceEnergy = 0.0f; //reduce player's energy

    public bool spawnSetMovement; //use this for fishes placed in level
    public string spawnMoveWaypoint; //

    public GameObject reticle;
        
    public int pointerIndex = 0; //set to -1 to make fish untargetable

    private FishStats mStats;
    private FishControllerBase mController;
    private Collectible mCollectible;
    private NGUIPointAt mPointIndicator;

    private HUD mHUD;
                
    public FishStats stats {
        get { return mStats; }
    }

    public FishControllerBase controller {
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

    public float PlayerContact(PlayerController pc, ControllerColliderHit hit) {
        float pushReturnSpeed;

        if(!invulnerable && pc.jumpSpecial.isActing) {
            Debug.Log("fish hurt");

            stats.curHit--;
            pushReturnSpeed = 0.0f;

            if(stats.curHit == 0) {
                pc.collectSensor.collector.AddToQueue(mCollectible);
            }
        }
        else {
            if(!pc.jumpSpecial.isActing) {
                //see if we can hurt the player
                if(contactReduceEnergy != 0.0f && pc.state != PlayerController.State.Stunned) {
                    pc.Hurt(contactReduceEnergy);
                }

                //stun?
                if(contactStun) {
                    pc.state = PlayerController.State.Stunned;
                }
            }

            pushReturnSpeed = pushForwardSpeed;
        }

        if(rigidbody != null && !rigidbody.isKinematic && pushBackSpeed > 0.0f)
            rigidbody.velocity = hit.moveDirection * pushBackSpeed;

        mController.OnPlayerContact(pc, hit);

        return pushReturnSpeed;
    }

    protected override void StateChanged() {
        //prev
        //switch(prevState) {
        //}
        
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
        
    }

    protected override void SpawnStart() {
        mCollectible.svalue = spawnType;

        reticleEnabled = false;
                
        mController.SpawnStart();

        state = StateNormal;


        if(spawnSetMovement) {
            mController.waypoint = spawnMoveWaypoint;
        }
        //SpawnFinish();
    }

    protected override void OnDespawned() {
        if(pointerIndex >= 0 && mPointIndicator != null) {
            mHUD.ReleasePointer(pointerIndex, mPointIndicator);
            mPointIndicator = null;
        }

        state = StateInvalid;
        reticleEnabled = false;
        gameObject.layer = Layers.fish;

        mController.curMoveMode = FishController.MoveMode.NumModes;
        
        mCollectible.collectFlagged = false;

        base.OnDespawned();
    }
    
    protected override void Awake() {
        base.Awake();

        reticleEnabled = false;

        mStats = GetComponent<FishStats>();
        mStats.changeCallback += OnStatChange;

        mController = GetComponent<FishControllerBase>();
        mController.moveModeChangedCallback += OnMoveModeChanged;

        mCollectible = GetComponent<Collectible>();
        mCollectible.collectedCallback += Release;
        
        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO != null ? hudGO.GetComponent<HUD>() : null;
    }

    void LateUpdate() {
        if(pointerIndex >= 0 && mPointIndicator == null && mHUD != null) {
            mPointIndicator = mHUD.AllocatePointer(pointerIndex);
            mPointIndicator.SetPOI(transform);
        }

    }
    
    void OnStatChange(FishStats stats) {
        if(stats.curHit == 0) {
            state = StateStunned;
        }
    }

    void OnMoveModeChanged(FishControllerBase ctrl) {
    }
}
