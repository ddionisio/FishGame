using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float moveSpeed;
    public float moveAirAccel;
    public float jumpSpeed;
    public float maxSpeed;
    public float slideSpeed;
    
    public float mass = 5.0f;
    public float swingSpeed = 0.05f;
    public float drag = 0.01f;
    public float maxAngleSpeed = 360.0f;
    public float bounceAngleSpeed = 15.0f;

    public float hookAimMidAirAngleLimit = 30.0f;

    public GameObject hookAimLine;
    public GameObject hookAimReticle;
    public float hookAimAngleSpeed = 30.0f;
        
    public RopeController rope;

    public LayerMask hookHitLayerMask;

    private CharacterController mCharCtrl;

    private Vector2 mCurVel;

    private const float slideLimitOfs = 0.1f;

    private float mAimTheta = Mathf.PI*0.5f;
    private float mTheta; //angle around hook to body
    private float mOmega; //current angle velocity

    private bool mInputEnabled = false;

    private bool mRopeEnabled = false;

    private bool mSliding = false;

    private CollisionFlags mCollFlags = CollisionFlags.None; //updated during move

    private float mRadianMaxSpeed;
    private float mRadianBounceSpeed;

    private float mRadianAimSpeed;

    private float mRadianCosSlideLimit;
    private float mRayCheckDistance;

    //private Vector3 mLastContactPoint = Vector3.zero;
    private RaycastHit mLastSlideHit;

    private Vector3 mFireDir;

    private bool mShowHookAim = false;
    private bool mFacingLeft = false;
    private bool mIsLastRopePoint = false; //true if we detached from rope and still mid-air

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
                        input.AddButtonCall(0, InputAction.Hook, OnHook);
                        input.AddButtonCall(0, InputAction.Jump, OnJump);
                        input.AddButtonCall(0, InputAction.Special, OnSpecial);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Hook, OnHook);
                        input.RemoveButtonCall(0, InputAction.Jump, OnJump);
                        input.RemoveButtonCall(0, InputAction.Special, OnSpecial);
                    }
                }

                if(!value)
                    ShowHookAim(false);
            }
        }
    }

    public bool isFacingLeft {
        get { return mFacingLeft; }

        set {
            if(mFacingLeft != value) {
                mFacingLeft = value;

                //flip animation

                mAimTheta *= -1.0f;
            }
        }
    }

    public void RopeShoot() {
        if(!mRopeEnabled) {
            Vector3 pos = transform.position;

            mRopeEnabled = true;

            //determine theta
            //last rope preserves previous angular velocity and theta reflected
            if(!mIsLastRopePoint) {
                mTheta = -mAimTheta;
                mFireDir = M8.MathUtil.Rotate(Vector2.up, mAimTheta);
                mOmega = 0.0f;
            }

            //determine position
            Vector3 startPos = pos + mFireDir*mCharCtrl.radius;

            rope.Fire(startPos, mFireDir);

            //Debug.Log("theta: " + (mTheta * Mathf.Rad2Deg));

            ShowHookAim(false);
        }
    }

    void RopeDetach() {
        mRopeEnabled = false;
        rope.Detach();
        ShowHookAim(mInputEnabled);
    }

    public void RopeRelease() {
        if(mRopeEnabled) {
            mIsLastRopePoint = true;

            //
            float radLim = hookAimMidAirAngleLimit*Mathf.Deg2Rad;
            mTheta = Mathf.Clamp(mTheta, -radLim, radLim);
            mFireDir = M8.MathUtil.Rotate(Vector2.up, mTheta);
            mTheta = -mTheta;
            //

            //convert angular velocity to linear velocity
            float r = ropeDistance;
            
            //TODO: config scalar?
            //float v = 2.0f * Mathf.Sqrt(-2.0f * Physics.gravity.y * r * (1.0f - Mathf.Cos(mTheta)));

            //mCurVel = Mathf.Sign(mTheta) * v * transform.right;

            mCurVel = transform.right*mOmega*r;

            //Debug.Log("theta: " + (Mathf.Rad2Deg * theta));
            //Debug.Log("vel: " + v);

            transform.up = Vector2.up;

            RopeDetach();
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }
    
    void Awake() {
        mCharCtrl = GetComponent<CharacterController>();

        rope.gameObject.SetActive(false);

        hookAimLine.SetActive(false);
        hookAimReticle.SetActive(false);

        mRadianMaxSpeed = maxAngleSpeed * Mathf.Deg2Rad;
        mRadianBounceSpeed = bounceAngleSpeed * Mathf.Deg2Rad;

        mRadianAimSpeed = hookAimAngleSpeed * Mathf.Deg2Rad;

        mRadianCosSlideLimit = Mathf.Cos((mCharCtrl.slopeLimit + slideLimitOfs) * Mathf.Deg2Rad);
        mRayCheckDistance = mCharCtrl.height * 0.5f + mCharCtrl.radius;
    }

    // Use this for initialization
    void Start() {
        inputEnabled = true;

        ShowHookAim(true);
    }
        
    // Update is called once per frame
    void FixedUpdate() {
#if UNITY_EDITOR
        mRadianMaxSpeed = maxAngleSpeed * Mathf.Deg2Rad;
        mRadianBounceSpeed = bounceAngleSpeed * Mathf.Deg2Rad;
        mRadianAimSpeed = hookAimAngleSpeed * Mathf.Deg2Rad;
        mRadianCosSlideLimit = Mathf.Cos((mCharCtrl.slopeLimit + slideLimitOfs) * Mathf.Deg2Rad);
        mRayCheckDistance = mCharCtrl.height * 0.5f + mCharCtrl.radius;
#endif

        InputManager input = Main.instance.input;

        float dt = Time.fixedDeltaTime;

        if(mRopeEnabled && rope.isAttached) {
            //update character movement with rope
            Vector3 ropeSPos = rope.startPosition;
            Vector3 pos = transform.position;
                        
            float len = ropeDistance;

            mOmega += mass * Physics.gravity.y * Mathf.Sin(mTheta) * dt / len - drag * mOmega;

            if(mInputEnabled) {
                mOmega += input.GetAxis(0, InputAction.DirX) * swingSpeed * Mathf.Cos(mTheta) / len;
            }

            mOmega = Mathf.Clamp(mOmega, -mRadianMaxSpeed, mRadianMaxSpeed);

            mTheta += mOmega * dt;

            //note: theta relative to y-axis, where 0 = up vector
            Vector3 dPos = new Vector3((ropeSPos.x + Mathf.Sin(mTheta) * len) - pos.x, (ropeSPos.y - Mathf.Cos(mTheta) * len) - pos.y, pos.z);

            mCollFlags = mCharCtrl.Move(dPos);

            Vector2 dUp = ropeSPos - pos;
            transform.up = dUp;

            rope.UpdateAttach(transform.position + transform.up * mCharCtrl.radius, hookHitLayerMask.value);

            //rope shrink/expand
            if(mInputEnabled) {
                float axisY = input.GetAxis(0, InputAction.DirY);
                if(axisY > 0.0f || (mCollFlags & CollisionFlags.Below) == 0)
                    rope.ExtendLength(-axisY, dt);
            }
        }
        else {
            bool isRopeShooting = mRopeEnabled && !rope.isAttached;
            if(isRopeShooting) {
                //rope is being fired, update until we hit a wall, or maximum length reached
                Vector3 startPos = transform.position + mFireDir * mCharCtrl.radius;
                if(rope.UpdateFire(startPos, mFireDir, dt, hookHitLayerMask.value)) {
                    if(rope.isAttached) {
                    }
                    else {
                        RopeDetach();
                    }
                }
            }

            if(mSliding) {
                if(mCharCtrl.isGrounded) {
                    mIsLastRopePoint = false;

                    if(!isRopeShooting && mInputEnabled && input.GetState(0, InputAction.Jump) == InputManager.State.Pressed) {
                        //jump based on surface angle
                        mCurVel = mLastSlideHit.normal;
                        mCurVel *= jumpSpeed;
                    }
                    else {
                        Vector3 n = mLastSlideHit.normal;
                        Vector3 v = new Vector3(n.x, -n.y, n.z);
                        Vector3.OrthoNormalize(ref n, ref v);
                        mCurVel = v;
                        mCurVel *= slideSpeed;
                    }
                }
                /*Vector2 n = mLastSlideHit.normal;
                mCurVel = M8.MathUtil.Slide(-Vector2.up, n);
                mCurVel *= slideSpeed;*/
            }
            else if(!isRopeShooting && mInputEnabled) {
                float axisX = input.GetAxis(0, InputAction.DirX);
    
                if(mCharCtrl.isGrounded) {
                    mIsLastRopePoint = false;

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

            //determine facing
            if(mCurVel.x != 0.0f)
                isFacingLeft = mCurVel.x < 0.0f;

            //aim stuff
            //adjust aim theta
            float axisY = input.GetAxis(0, InputAction.DirY);
            if(Mathf.Abs(axisY) > float.Epsilon) {
                const float pi_half = Mathf.PI * 0.5f;

                if(isFacingLeft) {
                    mAimTheta = Mathf.Clamp(mAimTheta + axisY * mRadianAimSpeed * dt, -pi_half, 0.0f);
                }
                else {
                    axisY *= -1.0f;

                    mAimTheta = Mathf.Clamp(mAimTheta + axisY * mRadianAimSpeed * dt, 0.0f, pi_half);
                }
            }

            UpdateHookAim();
        }
    }

    void OnSpecial(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
        }
    }

    void OnJump(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            if(mRopeEnabled) {
                //something fancy?
            }
            else if(mCharCtrl.isGrounded) {
                if(!mSliding) {
                    mCurVel.y = jumpSpeed;
                }
            }
        }
    }

    void OnHook(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            if(mRopeEnabled) {
                if(rope.isAttached)
                    RopeRelease();
            }
            else {
                RopeShoot();
            }
        }
    }

    void OnCollisionEnter(Collision coll) {
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        //mLastContactPoint = hit.point;

        if(mRopeEnabled) {
            //warning: mathematicians will cry when they see this
            float vel = Mathf.Abs(mOmega);

            Vector2 rpos = rope.startPosition;
            Vector2 hp = hit.point;
            Vector2 v1 = hit.normal;
            Vector2 v2 = hp - rpos;

            mOmega = M8.MathUtil.CheckSideSign(v2, v1)*(vel + mRadianBounceSpeed);

            Vector2 pos = transform.position;
            rope.curLength = (rpos - pos).magnitude - mCharCtrl.radius;
        }
    }

    private void ShowHookAim(bool show) {
        if(mShowHookAim != show) {
            mShowHookAim = show;

            hookAimLine.SetActive(mShowHookAim);
            hookAimReticle.SetActive(mShowHookAim);
        }
    }

    /// <summary>
    /// rad = angle relative to up vector
    /// </summary>
    private void UpdateHookAim() {
        if(mShowHookAim) {
            Vector2 up = mIsLastRopePoint ? new Vector2(mFireDir.x, mFireDir.y) : M8.MathUtil.Rotate(Vector2.up, mAimTheta);

            float r = mCharCtrl.radius + rope.maxLength;

            Vector3 pos = transform.position;

            RaycastHit rhit;
            if(Physics.Raycast(pos, up, out rhit, r, hookHitLayerMask.value)) {
                r = (pos - rhit.point).magnitude;
            }

            Vector3 hookPos = hookAimReticle.transform.localPosition;

            hookPos.x = up.x * r;
            hookPos.y = up.y * r;

            hookAimReticle.transform.localPosition = hookPos;
            hookAimReticle.transform.up = up;

            Vector3 hookLinePos = hookAimLine.transform.localPosition;
            hookLinePos.x = up.x * mCharCtrl.radius;
            hookLinePos.y = up.y * mCharCtrl.radius;

            Vector3 s = hookAimLine.transform.localScale;
            s.y = r - mCharCtrl.radius;

            hookAimLine.transform.localPosition = hookLinePos;
            hookAimLine.transform.localScale = s;
            hookAimLine.transform.up = up;
        }
    }

    private bool SlideCheck() {
        if(mCharCtrl.isGrounded && (mCollFlags & (CollisionFlags.CollidedSides | CollisionFlags.CollidedBelow)) != 0) {
            if(Physics.SphereCast(transform.position, mCharCtrl.radius, -Vector3.up, out mLastSlideHit, mRayCheckDistance)) {
                float dot = Vector2.Dot(mLastSlideHit.normal, Vector2.up);
                if(dot < mRadianCosSlideLimit)
                    return true;
            }
        }

        /*if(mCurVel.y < 0.0f) {
            //check with no volume
            if(Physics.Raycast(transform.position, -Vector3.up, out mLastSlideHit, mRayCheckDistance)) {
                float dot = Vector2.Dot(mLastSlideHit.normal, Vector2.up);
                if(dot < mRadianCosSlideLimit)
                    return true;
            }
            //check from last hit
            else if((mCollFlags & (CollisionFlags.CollidedSides | CollisionFlags.CollidedBelow)) != 0) {
                if(Physics.Raycast(mLastContactPoint + Vector3.up, -Vector3.up, out mLastSlideHit, mRayCheckDistance)) {
                    float dot = Vector2.Dot(mLastSlideHit.normal, Vector2.up);
                    if(dot < mRadianCosSlideLimit)
                        return true;
                }
            }
        }*/

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
