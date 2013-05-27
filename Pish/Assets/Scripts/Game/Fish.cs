using UnityEngine;
using System.Collections;

public class Fish : EntityBase {
    public const int StateNormal = 1;
    public const int StateStunned = 2;

    public enum BodyAnimState {
        normal,
        fear,
        stun,
        chase,

        NumBodyAnimStates
    }

    public GameObject reticle;

    public Transform bodyHolder;

    public tk2dAnimatedSprite bodyAnim;
    public tk2dAnimatedSprite finAnim;
    public tk2dAnimatedSprite tailAnim;

    public float rotSpeedScale = 10.0f;

    public tk2dSprite[] finCopies; //other fins to duplicate frame from finAnim

    public bool targetable = true;

    private FishStats mStats;
    private FishController mController;
    private Collectible mCollectible;
    private NGUIPointAt mPointIndicator;

    private HUD mHUD;

    private int[] mBodyAnimStateIds;

    private TransAnimRotWave[] mRotAnims;
    private float[] mRotAnimSpeeds;
    private bool mAvoiding = false;
        
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

    public float PlayerContact(PlayerController pc, ControllerColliderHit hit) {
        float pushBackSpeed;
        float pushSpeed = pc.fishContactSpeed;

        if(!mController.playerHitInvulnerable && pc.jumpSpecial.isActing) {
            Debug.Log("fish hurt");

            stats.curHit--;
            pushBackSpeed = 0.0f;

            if(stats.curHit == 0) {
                pc.collectSensor.collector.AddToQueue(mCollectible);
                //mController.Follow(pc.collectSensor.collector.transform);
            }
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

            pushBackSpeed = mController.playerContactPushSpeed;
        }

        rigidbody.velocity = hit.moveDirection * pushSpeed;

        return pushBackSpeed;
    }

    protected override void StateChanged() {
        //prev
        switch(prevState) {
            case StateStunned:
                transform.up = Vector3.up;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                break;
        }
        
        //new
        switch(state) {
            case StateNormal:
                mAvoiding = mController.flockUnit.numAvoid > 0;

                for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
                    mRotAnims[i].enabled = true;
                    mRotAnims[i].speed = mRotAnimSpeeds[i];
                }

                if(mAvoiding) {
                    bodyAnim.Play(mBodyAnimStateIds[(int)BodyAnimState.fear]);
                }
                else {
                    bodyAnim.Play(mBodyAnimStateIds[(int)BodyAnimState.normal]);
                }
                break;

            case StateStunned:
                gameObject.layer = Layers.collect;
                mController.curMoveMode = FishController.MoveMode.Fall;

                //stop fins and tail animation
                foreach(TransAnimRotWave rotAnims in mRotAnims) {
                    rotAnims.enabled = false;
                }

                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

                bodyAnim.Play(mBodyAnimStateIds[(int)BodyAnimState.stun]);
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
        
        //determine fin and tail
        //mSpawnFinInd != -1 mSpawnTailInd != -1

        //play first fin, copy clip to others
        //make sure mode is random frame
        finAnim.Play();
        tk2dSpriteCollectionData sprDat = finAnim.Collection;
        int sprInd = finAnim.spriteId;

        foreach(tk2dSprite spr in finCopies) {
            spr.SwitchCollectionAndSprite(sprDat, sprInd);
        }

        //make sure mode is random frame
        tailAnim.Play();

        state = StateNormal;

        //SpawnFinish();
    }

    protected override void OnDespawned() {
        if(mPointIndicator != null) {
            mHUD.ReleasePointer(mPointIndicator);
            mPointIndicator = null;
        }

        transform.up = Vector3.up;
        rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;

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

        reticleEnabled = false;

        mStats = GetComponent<FishStats>();
        mStats.changeCallback += OnStatChange;

        mController = GetComponent<FishController>();

        mCollectible = GetComponent<Collectible>();
        mCollectible.collectedCallback += Release;
        
        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO != null ? hudGO.GetComponent<HUD>() : null;

        mBodyAnimStateIds = new int[(int)BodyAnimState.NumBodyAnimStates];
        for(int i = 0; i < mBodyAnimStateIds.Length; i++) {
            mBodyAnimStateIds[i] = bodyAnim.GetClipIdByName(((BodyAnimState)i).ToString());
        }

        mRotAnims = GetComponentsInChildren<TransAnimRotWave>();
        mRotAnimSpeeds = new float[mRotAnims.Length];
        for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
            mRotAnimSpeeds[i] = mRotAnims[i].speed;
        }
    }

    void LateUpdate() {
        if(targetable && mPointIndicator == null && mHUD != null) {
            mPointIndicator = mHUD.AllocatePointer();
            mPointIndicator.SetPOI(transform);
        }

        /*public float normalRotDelay = 1.0f;
    public float fearRotDelay = 0.5f;*/
        switch(state) {
            case StateNormal:
                //being chased?
                bool avoid = mController.flockUnit.numAvoid > 0;
                if(mAvoiding != avoid) {
                    mAvoiding = avoid;

                    if(mAvoiding) {
                        bodyAnim.Play(mBodyAnimStateIds[(int)BodyAnimState.fear]);
                    }
                    else {
                        bodyAnim.Play(mBodyAnimStateIds[(int)BodyAnimState.normal]);
                    }

                    for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
                        mRotAnims[i].speed = Mathf.Sign(mRotAnimSpeeds[i])*(Mathf.Abs(mRotAnimSpeeds[i]) + rotSpeedScale*mController.flockUnit.curSpeed);
                    }
                }
                break;
        }

        //determine facing
        Vector3 bodyS = bodyHolder.localScale;
        bodyS.x = mController.flockUnit.dir.x > 0.0f ? -Mathf.Abs(bodyS.x) : Mathf.Abs(bodyS.x);
        bodyHolder.localScale = bodyS;
    }
    
    void OnStatChange(FishStats stats) {
        if(stats.curHit == 0) {
            state = StateStunned;
        }
    }
}
