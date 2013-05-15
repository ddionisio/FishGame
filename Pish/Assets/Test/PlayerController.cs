using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float moveSpeed;
    public float moveAirAccel;
    public float jumpSpeed;
    public float maxSpeed;
    public float slideSpeed;
    
    public float mass = 5.0f;
    public float angleSpeed = 0.05f;
    public float drag = 0.01f;
    public float maxAngleSpeed = 360.0f;
    public float bounceAngleSpeed = 15.0f;
        
    public RopeController rope;

    private CharacterController mCharCtrl;

    private Vector2 mCurVel;

    private const float slideLimitOfs = 0.1f;

    private float mTheta; //angle around hook to body
    private float mOmega; //current angle velocity

    private bool mInputEnabled = false;

    private bool mRopeEnabled = false;

    private bool mSliding = false;

    private CollisionFlags mCollFlags = CollisionFlags.None; //updated during move

    private float mRadianSpeed;
    private float mRadianMaxSpeed;
    private float mRadianBounceSpeed;

    private float mRadianSlideLimit;
    private float mRayCheckDistance;

    private Vector3 mLastContactPoint = Vector3.zero;
    private RaycastHit mLastSlideHit;

    /// <summary>
    /// Grab the distance from rope start to center of character.
    /// </summary>
    public float ropeDistance {
        get {
            return rope.curLength + mCharCtrl.radius;
        }
    }

    public bool inputEnabled {
        get {
            return mInputEnabled;
        }

        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;

                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Action, OnAction);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnAction);
                    }
                }
            }
        }
    }

    public void RopeShoot() {
        if(!mRopeEnabled) {
            Vector2 pos = Vector2.zero;

            mRopeEnabled = true;

            //determine theta

            mOmega = 0.0f;

            //determine position
            Vector2 startPos = transform.position;

            rope.Activate();
            rope.Attach(startPos);
        }
    }

    public void RopeRelease() {
        if(mRopeEnabled) {
            mRopeEnabled = false;

            rope.Detach();

            //convert angular velocity to linear velocity
            float r = ropeDistance;
            
            //TODO: config scalar?
            //float v = 2.0f * Mathf.Sqrt(-2.0f * Physics.gravity.y * r * (1.0f - Mathf.Cos(mTheta)));

            //mCurVel = Mathf.Sign(mTheta) * v * transform.right;

            mCurVel = transform.right*mOmega*r;

            //Debug.Log("theta: " + (Mathf.Rad2Deg * theta));
            //Debug.Log("vel: " + v);

            transform.up = Vector2.up;
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }
    
    void Awake() {
        mCharCtrl = GetComponent<CharacterController>();

        rope.gameObject.SetActive(false);

        mRadianSpeed = angleSpeed * Mathf.Deg2Rad;
        mRadianMaxSpeed = maxAngleSpeed * Mathf.Deg2Rad;
        mRadianBounceSpeed = bounceAngleSpeed * Mathf.Deg2Rad;

        mRadianSlideLimit = (mCharCtrl.slopeLimit + slideLimitOfs) * Mathf.Deg2Rad;
        mRayCheckDistance = mCharCtrl.height * 0.5f + mCharCtrl.radius;
    }

    // Use this for initialization
    void Start() {
        inputEnabled = true;

        RopeShoot();

        //mOmega = 0.0f;
       // mTheta = Mathf.Deg2Rad * 30.0f;
    }
        
    // Update is called once per frame
    void FixedUpdate() {
#if UNITY_EDITOR
        mRadianSpeed = angleSpeed * Mathf.Deg2Rad;
        mRadianMaxSpeed = maxAngleSpeed * Mathf.Deg2Rad;
        mRadianBounceSpeed = bounceAngleSpeed * Mathf.Deg2Rad;
        mRadianSlideLimit = (mCharCtrl.slopeLimit + slideLimitOfs) * Mathf.Deg2Rad;
        mRayCheckDistance = mCharCtrl.height * 0.5f + mCharCtrl.radius;
#endif

        InputManager input = Main.instance.input;

        float dt = Time.fixedDeltaTime;

        if(mRopeEnabled) {
            //update character movement with rope
            if(rope.isAttached) {
                Vector2 ropeSPos = rope.startPosition;
                Vector2 pos = transform.position;

                //rope shrink/expand
                if(mInputEnabled) {
                    float axisY = input.GetAxis(0, InputAction.DirY);
                    rope.ExtendLength(-axisY, dt);
                }

                float len = ropeDistance;

                mOmega += mass * Physics.gravity.y * Mathf.Sin(mTheta) * dt / len - drag * mOmega;

                if(mInputEnabled) {
                    mOmega += input.GetAxis(0, InputAction.DirX) * mRadianSpeed;
                }

                mOmega = Mathf.Clamp(mOmega, -mRadianMaxSpeed, mRadianMaxSpeed);

                mTheta += mOmega * dt;

                //note: theta relative to y-axis, where 0 = up vector
                Vector2 dPos = new Vector2((ropeSPos.x + Mathf.Sin(mTheta) * len) - pos.x, (ropeSPos.y - Mathf.Cos(mTheta) * len) - pos.y);

                mCollFlags = mCharCtrl.Move(dPos);

                transform.up = ropeSPos - pos;
            }
            else {
                //rope is being fired, update until we hit a wall, or maximum length reached
            }
        }
        else {          
            if(mSliding) {
                Vector3 n = mLastSlideHit.normal;
                Vector3 v = new Vector3(n.x, -n.y, n.z);
                Vector3.OrthoNormalize(ref n, ref v);
                mCurVel = v;
                mCurVel *= slideSpeed;
                /*Vector2 n = mLastSlideHit.normal;
                mCurVel = M8.MathUtil.Slide(-Vector2.up, n);
                mCurVel *= slideSpeed;*/
            }
            else if(mInputEnabled) {
                float axisX = input.GetAxis(0, InputAction.DirX);

                if(mCharCtrl.isGrounded) {
                    //move left/right
                    mCurVel.x = axisX * moveSpeed;
                }
                else {
                    if((axisX < 0.0f && mCurVel.x > -moveSpeed) || (axisX > 0.0f && mCurVel.x < moveSpeed)) {
                        mCurVel.x += input.GetAxis(0, InputAction.DirX) * moveAirAccel * dt;
                    }
                }
            }

            mCurVel.y += Physics.gravity.y * dt;

            M8.MathUtil.Limit(ref mCurVel, maxSpeed);
            
            mCollFlags = mCharCtrl.Move(mCurVel*dt);

            if((mCollFlags & CollisionFlags.Above) != 0) {
                if(mCurVel.y > 0.0f)
                    mCurVel.y = 0.0f;
            }
            else {
                SetSliding(SlideCheck());
            }
        }
    }

    void OnAction(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            if(mRopeEnabled) {
                if(rope.isAttached)
                    RopeRelease();
            }
            else {
                if(mSliding) {
                    SetSliding(false);

                    //jump based on surface angle
                    mCurVel = mLastSlideHit.normal;
                    mCurVel *= jumpSpeed;
                }
                //jump
                else if(mCharCtrl.isGrounded) {
                    mCurVel.y = jumpSpeed;
                }
                else {
                    //rope action
                }
            }
        }
    }

    void OnCollisionEnter(Collision coll) {
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        mLastContactPoint = hit.point;

        if(mRopeEnabled) {
            //warning: mathematicians will cry when they see this
            float vel = Mathf.Abs(mOmega) + Mathf.Sign(mOmega) * mRadianBounceSpeed;

            Vector2 rpos = rope.startPosition;
            Vector2 hp = hit.point;
            Vector2 v1 = hit.normal;
            Vector2 v2 = hp - rpos;

            mOmega = M8.MathUtil.CheckSideSign(v2, v1)*vel;

            rope.curLength = (transform.position - rope.startPosition).magnitude - mCharCtrl.radius;
        }
    }

    private bool SlideCheck() {
        if(mCurVel.y < 0.0f) {
            //check with no volume
            if(Physics.Raycast(transform.position, -Vector3.up, out mLastSlideHit, mRayCheckDistance)) {
                float dot = Vector2.Dot(mLastSlideHit.normal, Vector2.up);
                float theta = Mathf.Acos(dot);
                if(theta > mRadianSlideLimit)
                    return true;
            }
            //check from last hit
            else if((mCollFlags & (CollisionFlags.CollidedSides | CollisionFlags.CollidedBelow)) != 0) {
                if(Physics.Raycast(mLastContactPoint + Vector3.up, Vector3.up, out mLastSlideHit, mRayCheckDistance)) {
                    float dot = Vector2.Dot(mLastSlideHit.normal, Vector2.up);
                    float theta = Mathf.Acos(dot);
                    if(theta > mRadianSlideLimit)
                        return true;
                }
            }
        }

        return false;
    }

    private void SetSliding(bool sliding) {
        if(mSliding != sliding) {
            mSliding = sliding;

            /*if(mSliding)
                Debug.Log("slide!");*/

            //animation
        }
    }
}
