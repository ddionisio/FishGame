using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishController : FishControllerBase {
    public enum BodyAnimState {
        normal,
        fear,
        stun,
        chase
    }

    public float waypointApproxRadius = 0.1f;

    public float fallSpeedLimit = 2.5f;

    public bool playerChase = false; //chase player, sensor needs to be valid
    public float playerChaseMaxSpeed = 30.0f;

    //render data
    public Transform bodyHolder;

    public tk2dSpriteAnimator bodyAnim;
    public tk2dSpriteAnimator finAnim;
    public tk2dSpriteAnimator tailAnim;

    public float rotSpeedScale = 10.0f;

    public tk2dSprite[] finCopies; //other fins to duplicate frame from finAnim
        
    private FlockUnit mFlockUnit;

    private float mPrevFlockUnitMaxSpeed;

    private tk2dSpriteAnimationClip[] mBodyAnimStates;

    private TransAnimRotWave[] mRotAnims;
    private float[] mRotAnimSpeeds;
    private bool mAvoiding = false;

    public FlockUnit flockUnit {
        get { return mFlockUnit; }
    }

    public override void SpawnStart() {
        //determine fin and tail
        //mSpawnFinInd != -1 mSpawnTailInd != -1

        //play first fin, copy clip to others
        //make sure mode is random frame
        if(finAnim != null) {
            finAnim.Play();
            tk2dSpriteCollectionData sprDat = finAnim.Sprite.Collection;
            int sprInd = finAnim.Sprite.spriteId;

            foreach(tk2dSprite spr in finCopies) {
                spr.SetSprite(sprDat, sprInd);
            }
        }

        if(tailAnim != null) {
            //make sure mode is random frame
            tailAnim.Play();
        }
        //

        //initial movement animation
        mAvoiding = flockUnit.numAvoid > 0;

        for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
            mRotAnims[i].enabled = true;
            mRotAnims[i].speed = mRotAnimSpeeds[i];
        }

        SetBodyAnimNormal();
    }

    public override void Follow(Transform t) {
        Debug.Log("follow: " + t.name);
        curMoveMode = MoveMode.Chase;
        mFlockUnit.moveTarget = t;
    }

    public override void SetWanderData(Vector3 origin) {
        mFlockUnit.wanderOrigin = origin;
        mFlockUnit.wanderOriginLock = true;
    }

    public override void SetWanderData(Vector3 origin, float radius) {
        mFlockUnit.wanderOrigin = origin;
        mFlockUnit.wanderRadius = radius;
        mFlockUnit.wanderOriginLock = true;
    }

    protected override void OnMoveModeChange() {
        switch(prevMoveMode) {
            case MoveMode.Fall:
                transform.up = Vector3.up;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                break;

            case MoveMode.Chase:
                mFlockUnit.moveTarget = null;
                break;
        }
                
        //new
        switch(curMoveMode) {
            case MoveMode.Fall:
                collider.enabled = true;
                mFlockUnit.enabled = true;
                mFlockUnit.groupMoveEnabled = false;
                mFlockUnit.wanderEnabled = false;
                mFlockUnit.body.useGravity = true;
                mFlockUnit.maxSpeed = fallSpeedLimit;

                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

                //stop fins and tail animation
                foreach(TransAnimRotWave rotAnims in mRotAnims) {
                    rotAnims.enabled = false;
                }

                bodyAnim.Play(mBodyAnimStates[(int)BodyAnimState.stun]);
                break;

            case MoveMode.Idle:
                collider.enabled = true;
                mFlockUnit.enabled = true;
                mFlockUnit.groupMoveEnabled = true;
                mFlockUnit.wanderEnabled = true;
                mFlockUnit.body.useGravity = false;
                mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;

                SetBodyAnimNormal();
                break;

            case MoveMode.Path:
                collider.enabled = true;
                mFlockUnit.enabled = true;
                mFlockUnit.groupMoveEnabled = true;
                mFlockUnit.wanderEnabled = false;
                mFlockUnit.body.useGravity = false;
                mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;

                WaypointSetToNearestIndex();
                mFlockUnit.moveTarget = currentWaypoint;

                SetBodyAnimNormal();
                break;

            case MoveMode.Chase:
                collider.enabled = true;
                mFlockUnit.enabled = true;
                mFlockUnit.groupMoveEnabled = false;
                mFlockUnit.wanderEnabled = false;
                mFlockUnit.body.useGravity = false;
                mFlockUnit.maxSpeed = playerChaseMaxSpeed;

                bodyAnim.Play(mBodyAnimStates[(int)BodyAnimState.chase]);
                break;

            case MoveMode.NumModes:
                mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;
                mFlockUnit.ResetData();
                mFlockUnit.enabled = false;

                collider.enabled = false;
                break;
        }
    }

    protected override bool IsPathDone() {
        return mFlockUnit.moveTargetDistance <= waypointApproxRadius;
    }
        
    protected override void OnGotoPath(Transform t) {
        mFlockUnit.moveTarget = t;
    }

    void Awake() {
        mFlockUnit = GetComponent<FlockUnit>();
        mFlockUnit.enabled = false;
        mPrevFlockUnitMaxSpeed = mFlockUnit.maxSpeed;

        if(playerSensor != null) {
            playerSensor.addCallback += OnPlayerSensorAdded;
            playerSensor.removeCallback += OnPlayerSensorRemoved;
        }

        mBodyAnimStates = M8.tk2dUtil.GetSpriteClips(bodyAnim, typeof(BodyAnimState));

        mRotAnims = GetComponentsInChildren<TransAnimRotWave>(true);
        mRotAnimSpeeds = new float[mRotAnims.Length];
        for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
            mRotAnimSpeeds[i] = mRotAnims[i].speed;
        }
    }

    void LateUpdate() {
        switch(curMoveMode) {
            case MoveMode.Idle:
            case MoveMode.Path:
                //being chased?
                bool avoid = flockUnit.numAvoid > 0;
                if(mAvoiding != avoid) {
                    mAvoiding = avoid;

                    SetBodyAnimNormal();

                    for(int i = 0; i < mRotAnimSpeeds.Length; i++) {
                        mRotAnims[i].speed = Mathf.Sign(mRotAnimSpeeds[i]) * (Mathf.Abs(mRotAnimSpeeds[i]) + rotSpeedScale * flockUnit.curSpeed);
                    }
                }
                break;
        }

        //determine facing
        Vector3 bodyS = bodyHolder.localScale;
        bodyS.x = flockUnit.dir.x > 0.0f ? -Mathf.Abs(bodyS.x) : Mathf.Abs(bodyS.x);
        bodyHolder.localScale = bodyS;
    }
    
    void OnPlayerSensorAdded(PlayerController unit) {
        if(playerChase) {
            Debug.Log("chase: " + unit.name);

            if(curMoveMode == MoveMode.Chase) {
                mFlockUnit.moveTarget = unit.transform;
            }
            else if(curMoveMode != MoveMode.Fall || curMoveMode != MoveMode.NumModes) {
                curMoveMode = MoveMode.Chase;
                mFlockUnit.moveTarget = unit.transform;
            }
        }
    }

    void OnPlayerSensorRemoved(PlayerController unit) {
        Debug.Log("chase done: " + mFlockUnit.moveTarget + " unit: "+unit);
        if(mFlockUnit.moveTarget == unit.transform) {
            Debug.Log("changing back to: "+prevMoveMode);
            curMoveMode = prevMoveMode;
        }
    }

    void SetBodyAnimNormal() {
        if(mAvoiding) {
            bodyAnim.Play(mBodyAnimStates[(int)BodyAnimState.fear]);
        }
        else {
            bodyAnim.Play(mBodyAnimStates[(int)BodyAnimState.normal]);
        }
    }
}
