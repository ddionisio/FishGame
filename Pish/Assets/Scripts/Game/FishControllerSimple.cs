using UnityEngine;
using System.Collections;

public class FishControllerSimple : FishControllerBase {
    public enum AnimState {
        idle,
        move
    }

    public float waypointStayDelay;
    public float speed;

    public tk2dAnimatedSprite anim;
    public bool updateFacing = true;

    private enum MoveState {
        Moving,
        Wait,
        Done
    }

    private int[] mAnimIds;

    private float mDelayToDest;
    private float mCurTime;
    private Vector2 mDir;
    private Vector2 mDest;
    private MoveState mMoveState;

    public override void SpawnStart() {
        anim.Play(mAnimIds[(int)AnimState.idle]);
    }

    public override void OnPlayerContact(PlayerController pc, ControllerColliderHit hit) {
        Vector2 pos = transform.position;
        Vector2 otherPos = hit.point;

        Vector2 dpos = pos - otherPos;
        float mag = dpos.magnitude;

        float a = Mathf.Acos(Vector2.Dot(mDir, dpos) / mag);
        if(a > Mathf.PI * 0.5f) {
            WaypointReverse();
        }
    }

    public override void RefreshAnimation() {
        switch(curMoveMode) {
            case MoveMode.Idle:
                anim.Play(mAnimIds[(int)AnimState.idle]);
                break;

            case MoveMode.Path:
                anim.Play(mAnimIds[(int)AnimState.move]);
                break;
        }
    }

    protected override void OnMoveModeChange() {
        switch(curMoveMode) {
            case MoveMode.Idle:
                anim.Play(mAnimIds[(int)AnimState.idle]);
                break;

            case MoveMode.Path:
                OnGotoPath(currentWaypoint);
                break;
        }
    }

    protected override void OnGotoPath(Transform t) {
        mDest = t.position;
        RefreshDest();
    }

    protected override bool IsPathDone() {
        return mMoveState == MoveState.Done;
    }

    void Awake() {
        if(rigidbody != null)
            rigidbody.isKinematic = true;

        mAnimIds = M8.tk2dUtil.GenerateSpriteIds(anim, typeof(AnimState));
    }

    void FixedUpdate() {
        switch(curMoveMode) {
            case MoveMode.Path:
                float dt = Time.fixedDeltaTime;

                switch(mMoveState) {
                    case MoveState.Moving:
                        Vector3 pos = transform.position;
                        Vector3 dpos = Vector3.zero;

                        mCurTime += dt;
                        if(mCurTime < mDelayToDest) {
                            dpos.x = mDir.x * speed * dt;
                            dpos.y = mDir.y * speed * dt;
                        }
                        else {
                            dpos.x = mDest.x - pos.x;
                            dpos.y = mDest.y - pos.y;
                            mCurTime = mDelayToDest;
                                                        
                            mMoveState = waypointStayDelay > 0.0f ? MoveState.Wait : MoveState.Done;
                            mCurTime = 0.0f;

                            anim.Play(mAnimIds[(int)AnimState.idle]);
                        }

                        if(rigidbody != null) {
                            rigidbody.MovePosition(pos + dpos);
                        }
                        else {
                            transform.position = pos + dpos;
                        }
                        break;

                    case MoveState.Wait:
                        mCurTime += dt;
                        if(mCurTime >= waypointStayDelay)
                            mMoveState = MoveState.Done;
                        break;
                }
                break;
        }
    }

    void RefreshDest() {
        Vector2 pos = transform.position;
        Vector2 dpos = mDest - pos;
        float mag = dpos.magnitude;
        if(mag > 0.0f) {
            mDelayToDest = mag / speed;
            mDir = dpos / mag;
            mCurTime = 0.0f;

            if(updateFacing) {
                //determine facing
                //default facing right
                Vector3 bodyS = anim.scale;
                bodyS.x = mDir.x > 0.0f ? -Mathf.Abs(bodyS.x) : Mathf.Abs(bodyS.x);
                anim.scale = bodyS;
            }

            mMoveState = MoveState.Moving;

            anim.Play(mAnimIds[(int)AnimState.move]);
        }
        else {
            mCurTime = mDelayToDest = 0.0f;
            mMoveState = MoveState.Done;

            anim.Play(mAnimIds[(int)AnimState.idle]);
        }
    }
}
