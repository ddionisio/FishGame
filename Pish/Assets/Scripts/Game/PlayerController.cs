using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public delegate void OnStateChange(PlayerController pc, State prevState);
    public delegate void OnHurt(PlayerController pc, float energy);
    public delegate void OnTrigger(Collider c);

    public enum State {
        None,
        Normal,
        RopeShoot,
        Roping,
        Stunned,

        NumStates
    }

    public enum Special {
        Boost,

        NumSpecials
    }

    public float moveSpeed;
    public float moveAirAccel;
    public float jumpSpeed;
    public float maxSpeed;
    public float slideSpeed;

    public float hurtSpeed; //speed at which we are going to be hurt
    public float hurtBounceOffSpeed;
    public float hurtEnergy = 1.0f;
    public float hurtDelay = 2.0f; //revert to normal after given seconds

    public float mass = 5.0f;
    public float swingRevolution = 0.05f;
    public float drag = 0.01f;
    public float maxAngleSpeed = 360.0f;
    public float bounceAngleSpeed = 15.0f;

    public float hookAimMidAirAngleLimit = 30.0f;

    public PlayerAnimator animator;

    public GameObject hookAimLine;
    public GameObject hookAimReticle;
    public float hookAimAngleSpeed = 30.0f;
    public float hookAimLineScale = 1.0f;
    public Color hookAimValidColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);
    public Color hookAimInvalidColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);

    public SoundPlayer hookShotSound;
    public SoundPlayer hookShotHitSound;

    public SoundPlayer hitSound;

    public RopeController rope;
    public FishSensor fishSensor;
    public CollectibleSensor collectSensor;

    public SpecialBase[] specials; //corresponds to Special enum
    public SpecialBase jumpSpecial;

    public tk2dSpriteAnimator puff;

    public LayerMask terrainMask;
    public LayerMask fishMask;

    public event OnStateChange stateCallback;
    public event OnHurt hurtCallback;

    /// <summary>
    /// Called when we trigger an object in layer: PlayerTrigger
    /// </summary>
    public event OnTrigger playerTriggerEnterCallback;

    private State mState = State.None;

    private Special mCurSpecialType = Special.NumSpecials;
    private SpecialBase mCurSpecial = null;

    private CharacterController mCharCtrl;

    private Vector2 mCurVel;
    private Vector2 mCurInputAxis;

    private const float slideLimitOfs = 0.1f;

    private float mAimTheta = Mathf.PI * 0.5f;
    private float mTheta; //angle around hook to body
    private float mOmega; //current angle velocity

    private bool mInputEnabled = false;

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
    private float mIsLastRopeSqMag;

    private float mCollectorAttachDist;

    private SpecialActionTrigger mSpecialTrigger;

    /// <summary>
    /// This is used by SpecialActionTrigger when player enters an action area
    /// </summary>
    public SpecialActionTrigger specialTrigger {
        get { return mSpecialTrigger; }
        set { mSpecialTrigger = value; }
    }

    public Vector2 curInputAxis {
        get { return mCurInputAxis; }
    }

    public CharacterController charCtrl {
        get { return mCharCtrl; }
    }

    public bool isSpecialActive {
        get { return mCurSpecial != null && mCurSpecial.isActing; }
    }

    public bool isJumpSpecialActive {
        get { return jumpSpecial != null && jumpSpecial.isActing; }
    }

    public Vector2 curVelocity {
        get { return mCurVel; }
        set { mCurVel = value; }
    }

    public float curAngle {
        get { return mTheta; }
    }

    public float curAngleVelocity {
        get { return mOmega; }
        set { mOmega = value; }
    }

    public State state {
        get { return mState; }
        set {
            if(mState != value) {
                State lastState = mState;

                //shutdown prev state
                switch(mState) {
                    case State.Normal:
                        ShowHookAim(false);
                        break;

                    case State.RopeShoot:
                        if(value != State.Roping)
                            rope.Detach();
                        break;

                    case State.Roping:
                        if(value == State.Stunned)
                            mCurVel = GetLinearFromOmega();

                        transform.up = Vector2.up;
                        rope.Detach();
                        break;

                    case State.Stunned:
                        StopCoroutine("StunCounter");
                        break;
                }

                mState = value;

                //init new state
                switch(mState) {
                    case State.Normal:
                        ShowHookAim(mInputEnabled);
                        break;

                    case State.RopeShoot:
                        if(hookShotSound != null)
                            hookShotSound.Play();

                        if(mCharCtrl.isGrounded)
                            mCurVel.x = 0.0f;

                        animator.particleEnable = false;
                        break;

                    case State.Roping:
                        if(hookShotHitSound != null)
                            hookShotHitSound.Play();

                        animator.mode = PlayerAnimator.Mode.Normal;
                        animator.RevertUpCancel();
                        animator.transform.up = Vector3.up;
                        break;

                    case State.Stunned:
                        if(isSpecialActive)
                            mCurSpecial.ActStop(this);

                        if(isJumpSpecialActive)
                            jumpSpecial.ActStop(this);

                        StartCoroutine(StunCounter());
                        break;
                }

                if(stateCallback != null)
                    stateCallback(this, lastState);
            }
        }
    }

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

                if(!value) {
                    mCurInputAxis = Vector2.zero;
                    mCurVel.x = 0.0f;
                    //ShowHookAim(false);
                }
            }
        }
    }

    public bool isFacingLeft {
        get { return mFacingLeft; }

        set {
            if(mFacingLeft != value) {
                mFacingLeft = value;

                //flip animation
                animator.isFacingLeft = mFacingLeft;

                mAimTheta *= -1.0f;
            }
        }
    }

    public void Hurt(float hurtEnergy) {
        if(hurtCallback != null) {
            hurtCallback(this, hurtEnergy);
        }
    }

    public void RopeShoot() {
        if(state == State.Normal) {
            Vector3 pos = transform.position;

            //determine theta
            mTheta = -mAimTheta;
            mFireDir = M8.MathUtil.Rotate(Vector2.up, mAimTheta);

            if(!mIsLastRopePoint) {
                mOmega = 0.0f;
            }
            else {
                if(mIsLastRopeSqMag != 0.0f)
                    mOmega *= mCurVel.sqrMagnitude / mIsLastRopeSqMag;
            }

            //determine position
            Vector3 startPos = pos + mFireDir * mCharCtrl.radius;

            rope.Fire(startPos, mFireDir);

            //Debug.Log("theta: " + (mTheta * Mathf.Rad2Deg));

            state = State.RopeShoot;
        }
    }

    public void RopeRelease() {
        if(state == State.Roping) {
            mIsLastRopePoint = true;

            //convert angular velocity to linear velocity
            mCurVel = GetLinearFromOmega();

            mIsLastRopeSqMag = mCurVel.sqrMagnitude;

            isFacingLeft = mCurVel.x < 0.0f;

            float radLim = hookAimMidAirAngleLimit * Mathf.Deg2Rad;
            mTheta = Mathf.Clamp(mTheta, -radLim, radLim);
            mAimTheta = mTheta;

            //Debug.Log("theta: " + (Mathf.Rad2Deg * theta));
            //Debug.Log("vel: " + v);

            transform.up = Vector2.up;

            state = State.Normal;

            animator.mode = PlayerAnimator.Mode.Spin;
            animator.state = PlayerAnimator.State.roll;
            animator.spinSpeed = Mathf.Abs(8.0f * mOmega * Mathf.Rad2Deg);
        }
    }

    void OnDestroy() {
        inputEnabled = false;

        stateCallback = null;
        hurtCallback = null;
        playerTriggerEnterCallback = null;
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

        foreach(SpecialBase s in specials) {
            s.gameObject.SetActive(false);
        }

        fishSensor.mask = fishMask;

        mCollectorAttachDist = collectSensor.collector.ownerAttach.localPosition.magnitude;
    }

    // Use this for initialization
    void Start() {
        SpecialsInit();
    }

    /*void Update() {
    }*/

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

        if(mInputEnabled) {
            mCurInputAxis.x = input.GetAxis(0, InputAction.DirX);
            mCurInputAxis.y = input.GetAxis(0, InputAction.DirY);
        }
        else {
            mCurInputAxis = Vector2.zero;
        }

        float dt = Time.fixedDeltaTime;

        switch(mState) {
            case State.Stunned:
                ShowHookAim(false);

                if((mCurInputAxis.x < 0.0f && mCurVel.x > -moveSpeed) || (mCurInputAxis.x > 0.0f && mCurVel.x < moveSpeed)) {
                    mCurVel.x += mCurInputAxis.x * moveAirAccel * dt;
                }

                UpdateCharacterMove(dt, true, false, false);

                animator.state = PlayerAnimator.State.stunned;

                UpdateSpeedAnimation(false);
                break;

            case State.Normal:
                if(isSpecialActive) {
                    mCurSpecial.ActUpdate(this, dt);
                    UpdateCharacterMove(dt, !mCurSpecial.lockGravity, false, false);

                    ShowHookAim(!mCurSpecial.isActing && mInputEnabled);
                }
                else if(isJumpSpecialActive) {
                    jumpSpecial.ActUpdate(this, dt);
                    UpdateCharacterMove(dt, !jumpSpecial.lockGravity, false, false);

                    ShowHookAim(!jumpSpecial.isActing && mInputEnabled);
                }
                else {
                    if(mSliding) {
                        if(mCharCtrl.isGrounded) {
                            mIsLastRopePoint = false;

                            if(mInputEnabled && input.GetState(0, InputAction.Jump) == InputManager.State.Pressed) {
                                //jump based on surface angle
                                mCurVel = mLastSlideHit.normal;
                                mCurVel *= jumpSpeed;
                            }
                            else {
                                UpdateSliding();
                            }
                        }
                    }
                    else {
                        //move left/right
                        if(mInputEnabled) {
                            if(mCharCtrl.isGrounded) {
                                mIsLastRopePoint = false;

                                //move left/right
                                mCurVel.x = mCurInputAxis.x * moveSpeed;
                            }
                            else {
                                if((mCurInputAxis.x < 0.0f && mCurVel.x > -moveSpeed) || (mCurInputAxis.x > 0.0f && mCurVel.x < moveSpeed)) {
                                    mCurVel.x += mCurInputAxis.x * moveAirAccel * dt;
                                }
                            }
                        }
                    }

                    //fall and update character position
                    UpdateCharacterMove(dt, true, true, true);

                    SetSliding(SlideCheck());

                    UpdateNormalAnimation();

                    UpdateSpeedAnimation(false);
                }
                break;

            case State.RopeShoot:
                //rope is being fired, update until we hit a wall, or maximum length reached
                Vector3 startPos = transform.position + mFireDir * mCharCtrl.radius;
                if(rope.UpdateFire(startPos, mFireDir, dt, terrainMask.value)) {
                    state = rope.isAttached ? State.Roping : State.Normal;
                }

                if(mSliding) {
                    if(mCharCtrl.isGrounded) {
                        mIsLastRopePoint = false;
                        UpdateSliding();
                    }
                }

                //fall and update character position
                UpdateCharacterMove(dt, true, true, false);

                SetSliding(SlideCheck());

                UpdateNormalAnimation();
                break;

            case State.Roping:
                //update character movement with rope
                Vector2 ropeSPos = rope.startPosition;
                Vector2 pos = transform.position;

                float len = ropeDistance;

                mOmega += mass * Physics.gravity.y * Mathf.Sin(mTheta) * dt / len - drag * mOmega;

                if(isSpecialActive) {
                    mCurSpecial.ActUpdate(this, dt);
                }
                else if(isJumpSpecialActive) {
                    jumpSpecial.ActUpdate(this, dt);
                }
                else if(mInputEnabled) {
                    //mOmega += mCurInputAxis.x * swingSpeed * Mathf.Cos(mTheta) / len;
                    mOmega += InputOmega(len);
                }

                mOmega = Mathf.Clamp(mOmega, -mRadianMaxSpeed, mRadianMaxSpeed);

                mTheta += mOmega * dt;

                //note: theta relative to y-axis, where 0 = up vector
                Vector3 dPos = new Vector3((ropeSPos.x + Mathf.Sin(mTheta) * len) - pos.x, (ropeSPos.y - Mathf.Cos(mTheta) * len) - pos.y, 0.0f);

                mCollFlags = mCharCtrl.Move(dPos);

                //make sure we are still in roping state... (may have collided with something that stuns us)
                if(mState == State.Roping) {
                    Vector2 dUp = ropeSPos - pos;
                    transform.up = dUp;

                    Vector3 curUp = transform.up;

                    if(rope.UpdateAttach(transform.position + curUp * mCharCtrl.radius, terrainMask.value)) {
                        //refresh theta
                        RefreshTheta();

                        //reposition
                        //make sure we are still in roping state... (may have collided with something that stuns us)
                        pos = transform.position;
                        dPos = new Vector3((ropeSPos.x + Mathf.Sin(mTheta) * len) - pos.x, (ropeSPos.y - Mathf.Cos(mTheta) * len) - pos.y, 0.0f);
                        mCollFlags = mCharCtrl.Move(dPos);
                        if(mState != State.Roping)
                            break;
                    }

                    //rope shrink/expand
                    if(mInputEnabled) {
                        if((mCurInputAxis.y > 0.0f && (mCollFlags & CollisionFlags.Above) == 0) || (mCurInputAxis.y < 0.0f && (mCollFlags & CollisionFlags.Below) == 0))
                            rope.ExtendLength(-mCurInputAxis.y, dt);
                    }

                    collectSensor.collector.ownerAttach.transform.localPosition = -curUp * mCollectorAttachDist;

                    Vector2 vv = curUp;
                    M8.MathUtil.DirCap(Vector2.up, ref vv, 180);
                    float scale = mFacingLeft ? -vv.x : vv.x;
                    scale = scale < 0.0f ? scale + 0.5f : scale - 0.5f;
                    int ind = Mathf.Clamp(Mathf.RoundToInt(scale * animator.swingStates.Length) + animator.swingStates.Length / 2, 0, animator.swingStates.Length - 1);
                    animator.state = isJumpSpecialActive ? animator.swingBoostStates[ind] : animator.swingStates[ind];

                    UpdateSpeedAnimation(true);
                }
                break;
        }
    }

    //only call if roping
    void RefreshTheta() {
        Vector2 dir = transform.position - rope.startPosition;
        float mag = dir.magnitude;
        dir /= mag;
        float a = Mathf.Acos(Vector2.Dot(-Vector2.up, dir));
        switch(M8.MathUtil.CheckSide(dir, -Vector2.up)) {
            case M8.MathUtil.Side.Left:
                mTheta = -Mathf.Abs(a);
                break;

            case M8.MathUtil.Side.Right:
                mTheta = Mathf.Abs(a);
                break;
        }
    }

    void UpdateNormalAnimation() {
        //determine animation
        if(animator.mode == PlayerAnimator.Mode.Normal) {
            if(mSliding) {
                animator.state = PlayerAnimator.State.slide;
            }
            else if(mCharCtrl.isGrounded) {
                animator.state = mCurVel.x != 0.0f ? PlayerAnimator.State.move : PlayerAnimator.State.idle;
            }
            else {
                animator.state = mCurVel.y > 0.0f ? PlayerAnimator.State.jump : PlayerAnimator.State.fall;
            }
        }
    }

    void UpdateCharacterMove(float dt, bool updateGravity, bool haltVelocityOnCollide, bool updateHookAim) {
        if(updateGravity)
            mCurVel.y += Physics.gravity.y * dt;

        M8.MathUtil.Limit(ref mCurVel, maxSpeed);

        mCollFlags = mCharCtrl.Move(mCurVel * dt);

        if(haltVelocityOnCollide) {
            if((mCollFlags & CollisionFlags.Above) != 0) {
                if(mCurVel.y > 0.0f)
                    mCurVel.y = 0.0f;
            }

            if((mCollFlags & CollisionFlags.Sides) != 0) {
                mCurVel.x = 0.0f;
            }
        }

        //determine facing
        if(mCurVel.x != 0.0f) {
            isFacingLeft = mCurVel.x < 0.0f;
        }

        //set collector attach offset
        if(mCharCtrl.isGrounded) {
            if(mCharCtrl.isGrounded) {
                if(mFacingLeft) {
                    collectSensor.collector.ownerAttach.transform.localPosition = transform.right * mCollectorAttachDist;
                }
                else {
                    collectSensor.collector.ownerAttach.transform.localPosition = -transform.right * mCollectorAttachDist;
                }
            }
            else {
                collectSensor.collector.ownerAttach.transform.localPosition = -transform.up * mCollectorAttachDist;
            }

            if(animator.mode == PlayerAnimator.Mode.Spin) {
                animator.mode = PlayerAnimator.Mode.Normal;
                animator.transform.up = Vector3.up;
            }
        }

        if(updateHookAim) {
            //aim stuff
            //adjust aim theta
            if(Mathf.Abs(mCurInputAxis.y) > float.Epsilon) {
                const float pi_half = Mathf.PI * 0.5f;

                if(isFacingLeft) {
                    mAimTheta = Mathf.Clamp(mAimTheta + mCurInputAxis.y * mRadianAimSpeed * dt, -pi_half, 0.0f);
                }
                else {
                    float axisY = -1.0f * mCurInputAxis.y;

                    mAimTheta = Mathf.Clamp(mAimTheta + axisY * mRadianAimSpeed * dt, 0.0f, pi_half);
                }
            }

            UpdateHookAim();
        }
    }

    void UpdateSliding() {
        Vector3 n = mLastSlideHit.normal;
        Vector3 v = new Vector3(n.x, -n.y, n.z);
        Vector3.OrthoNormalize(ref n, ref v);
        mCurVel = v;
        mCurVel *= slideSpeed;
    }

    void OnSpecial(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            if(mSpecialTrigger != null) {
                if(mSpecialTrigger.criteriaValid) {
                    mSpecialTrigger.Action();
                }
            }
            else if(mCurSpecial != null) {
                if((mState == State.Roping || mState == State.Normal)
                && !(isSpecialActive || isJumpSpecialActive)) {
                    mCurSpecial.Act(this);
                }
            }

        }
    }

    void OnJump(InputManager.Info data) {
        if(!(isSpecialActive || isJumpSpecialActive)) {
            if(data.state == InputManager.State.Pressed) {
                switch(mState) {
                    case State.Roping:
                        if(jumpSpecial != null)
                            jumpSpecial.Act(this);
                        break;

                    case State.Normal:
                        if(mCharCtrl.isGrounded) {
                            if(!mSliding) {
                                mCurVel.y = jumpSpeed;
                            }
                        }
                        else if(jumpSpecial != null) {
                            if(jumpSpecial.Act(this))
                                mIsLastRopePoint = false;
                        }
                        break;
                }
            }
        }
    }

    void OnHook(InputManager.Info data) {
        if(data.state == InputManager.State.Pressed) {
            switch(mState) {
                case State.Roping:
                    RopeRelease();
                    break;

                case State.Normal:
                    if(!(isSpecialActive || isJumpSpecialActive)) {
                        RopeShoot();
                    }
                    break;
            }
        }
    }

    void OnTriggerEnter(Collider coll) {
        int layer = coll.gameObject.layer;

        if(layer == Layers.playerTrigger) {
            if(playerTriggerEnterCallback != null)
                playerTriggerEnterCallback(coll);
        }
        else if(layer == Layers.specialTrigger) {
            if(mSpecialTrigger == null) {
                mSpecialTrigger = coll.GetComponent<SpecialActionTrigger>();
                if(mSpecialTrigger != null)
                    mSpecialTrigger.playerController = this;
            }
            else if(mSpecialTrigger.gameObject != coll.gameObject) {
                SpecialActionTrigger trigger = coll.GetComponent<SpecialActionTrigger>();
                if(trigger != null) {
                    mSpecialTrigger.playerController = null;
                    mSpecialTrigger = trigger;
                    mSpecialTrigger.playerController = this;
                }
            }
        }
    }

    void OnTriggerExit(Collider coll) {
        int layer = coll.gameObject.layer;

        if(layer == Layers.specialTrigger) {
            if(mSpecialTrigger != null && mSpecialTrigger.gameObject == coll.gameObject) {
                mSpecialTrigger.playerController = null;
                mSpecialTrigger = null;
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        switch(mState) {
            case State.Roping:
                if(hit.gameObject.layer == Layers.spike) {
                    LevelController levelCtrl = LevelController.instance;

                    state = State.Stunned;

                    transform.position = hit.point + hit.normal * mCharCtrl.radius;

                    //bounce off
                    mCurVel = levelCtrl.SpikeBounceOff(hit, mCurVel);

                    Hurt(levelCtrl.spikeHurtAmount);
                }
                else if((terrainMask & (1 << hit.gameObject.layer)) != 0) {
                    RopingBounce(hit.point, hit.normal);
                }
                else if((fishMask & (1 << hit.gameObject.layer)) != 0) {
                    Vector2 vel = GetLinearFromOmega();

                    RopingBounce(hit.point, hit.normal);

                    FishContact(vel, hit);
                }
                break;

            case State.Stunned:
            case State.Normal:
            case State.RopeShoot:
                if(hit.gameObject.layer == Layers.spike) {
                    LevelController levelCtrl = LevelController.instance;

                    state = State.Stunned;

                    //bounce off
                    mCurVel = levelCtrl.SpikeBounceOff(hit, mCurVel);

                    Hurt(levelCtrl.spikeHurtAmount);
                }
                //check if we hit a wall and determine if we are hurt
                else if((terrainMask & (1 << hit.gameObject.layer)) != 0) {
                    float speedSq = mCurVel.sqrMagnitude;
                    if(speedSq >= hurtSpeed * hurtSpeed && !mCharCtrl.isGrounded) {
                        Debug.Log("ouch");

                        //bounce off
                        Vector2 rV = M8.MathUtil.Reflect(mCurVel, hit.normal);
                        mCurVel = rV.normalized * hurtBounceOffSpeed;

                        State prevState = state;

                        state = State.Stunned;

                        if(prevState != State.Stunned)
                            Hurt(hurtEnergy);
                    }
                    else if(state != State.RopeShoot) {
                        state = State.Normal;
                    }
                }
                else if((fishMask & (1 << hit.gameObject.layer)) != 0) {
                    float spd = FishContact(mCurVel, hit);

                    //bounce off
                    Vector2 moveDir = hit.moveDirection;
                    mCurVel = -moveDir * spd;
                }
                break;
        }
    }

    IEnumerator StunCounter() {
        yield return new WaitForSeconds(hurtDelay);

        if(mState == State.Stunned)
            state = State.Normal;
    }

    private Vector2 GetLinearFromOmega() {
        //

        //convert angular velocity to linear velocity
        float r = ropeDistance;

        //TODO: config scalar?
        //float v = 2.0f * Mathf.Sqrt(-2.0f * Physics.gravity.y * r * (1.0f - Mathf.Cos(mTheta)));

        //mCurVel = Mathf.Sign(mTheta) * v * transform.right;

        return transform.right * mOmega * r;
    }

    private void UpdateSpeedAnimation(bool isRoping) {
        if(!mCharCtrl.isGrounded && mCollFlags == CollisionFlags.None) {
            float spd = isRoping ? Mathf.Abs(mOmega * ropeDistance) : mCurVel.magnitude;
            animator.particleEnable = spd >= hurtSpeed;
        }
        else {
            animator.particleEnable = false;
        }
    }

    void PuffAt(Vector2 pos) {
        if(puff != null) {
            Vector3 puffPos = puff.transform.position;
            puff.transform.position = new Vector3(pos.x, pos.y, puffPos.z);

            if(puff.gameObject.activeSelf) {
                puff.Play();
            }
            else {
                puff.gameObject.SetActive(true);
            }
        }
    }

    //return speed
    private float FishContact(Vector2 velocity, ControllerColliderHit hit) {
        Fish fish = hit.gameObject.GetComponent<Fish>();

        if(jumpSpecial != null && jumpSpecial.isActing) {
            PuffAt(hit.point);

            if(hitSound != null)
                hitSound.Play();
        }

        return fish.PlayerContact(this, hit);
    }

    private float InputOmega(float len) {
        return (mCurInputAxis.x * swingRevolution) / (2.0f * Mathf.PI * len);
    }

    private void RopingBounce(Vector2 contactPt, Vector2 normal) {
        //warning: mathematicians will cry when they see this
        float vel = Mathf.Abs(mOmega);

        Vector2 rpos = rope.startPosition;
        Vector2 v2 = contactPt - rpos;

        //float inpOmega = Mathf.Abs(InputOmega(ropeDistance));

        mOmega = M8.MathUtil.CheckSideSign(v2, normal) * (vel + mRadianBounceSpeed);// + inpOmega);

        Vector2 pos = transform.position;
        rope.curLength = (rpos - pos).magnitude - mCharCtrl.radius;
    }

    private void SpecialsInit() {
        if(mCurSpecialType != Special.NumSpecials) {
            int specialInd = (int)mCurSpecialType;
            if(specialInd < specials.Length) {
                mCurSpecial = specials[specialInd];
                mCurSpecial.gameObject.SetActive(true);
                mCurSpecial.Init(this);
            }
        }
        else {
            mCurSpecial = null;
        }

        if(jumpSpecial != null)
            jumpSpecial.Init(this);
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
            Vector2 up = M8.MathUtil.Rotate(Vector2.up, mAimTheta);

            float r = mCharCtrl.radius + rope.maxLength;

            Vector3 pos = transform.position;

            RaycastHit rhit;
            bool isHit = Physics.Raycast(pos, up, out rhit, r, terrainMask.value);

            if(isHit) {
                if(rope.IsInvalid(rhit.collider.gameObject.layer)) {
                    isHit = false;
                    hookAimReticle.SetActive(false);
                }
                else {
                    hookAimReticle.SetActive(true);
                }

                r = (pos - rhit.point).magnitude;

                Vector3 hookPos = hookAimReticle.transform.localPosition;

                hookPos.x = up.x * r;
                hookPos.y = up.y * r;

                hookAimReticle.transform.localPosition = hookPos;
            }
            else {
                hookAimReticle.SetActive(false);
            }

            Vector3 hookLinePos = hookAimLine.transform.localPosition;
            hookLinePos.x = up.x * mCharCtrl.radius;
            hookLinePos.y = up.y * mCharCtrl.radius;

            Vector3 s = hookAimLine.transform.localScale;
            s.y = r - mCharCtrl.radius;

            hookAimLine.transform.localPosition = hookLinePos;
            hookAimLine.transform.localScale = s;
            hookAimLine.transform.up = up;

            //update line tile scale
            Material lineMat = hookAimLine.renderer.material;
            lineMat.SetFloat("tileY", s.y / hookAimLineScale);
            lineMat.SetColor("modColor", isHit ? hookAimValidColor : hookAimInvalidColor);
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
