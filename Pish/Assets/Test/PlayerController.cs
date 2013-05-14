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

    public RopeController rope;

    private CharacterController mCharCtrl;

    private Vector2 mCurVel;

    private float mTheta; //angle around hook to body
    private float mOmega; //current angle velocity

    private bool mInputEnabled = false;

    private bool mRopeEnabled = false;

    private bool mSliding = false;

    private CollisionFlags mCollFlags = CollisionFlags.None; //updated during move

    private float mRadianSpeed;
    private float mRadianMaxSpeed;

    private float mRadianSlideLimit;
    private float mRayCheckDistance;

    private Vector3 mLastContactPoint = Vector3.zero;
    private RaycastHit mLastSlideHit;

    /// <summary>
    /// Grab the distance from rope start to center of character.
    /// </summary>
    public float ropeDistance {
        get {
            return rope.ropeLength + mCharCtrl.radius;
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

    public bool RopeShoot() {
        //determine if we hit a wall based on max shoot length
        bool ret = true;

        Vector2 pos = Vector2.zero;

        mRopeEnabled = true;

        //determine theta

        mOmega = 0.0f;

        //determine position

        rope.Activate(pos);

        return ret;
    }

    public void RopeRelease() {
        mRopeEnabled = false;

        rope.Deactivate();

        //convert angular velocity to linear velocity
        float r = ropeDistance;
        float theta = Mathf.Acos(Vector2.Dot(transform.up, -Vector2.up));

        //TODO: config scalar?
        float v = 2.0f * Mathf.Sqrt(-2.0f * Physics.gravity.y * rope.ropeLength * (1.0f - Mathf.Cos(mTheta)));

        mCurVel = Mathf.Sign(mTheta) * v * transform.right;

        //Debug.Log("theta: " + (Mathf.Rad2Deg * theta));
        //Debug.Log("vel: " + v);

        transform.up = Vector2.up;
    }

    void OnDestroy() {
        inputEnabled = false;
    }
    
    void Awake() {
        mCharCtrl = GetComponent<CharacterController>();

        rope.gameObject.SetActive(false);

        mRadianSpeed = angleSpeed * Mathf.Deg2Rad;
        mRadianMaxSpeed = maxAngleSpeed * Mathf.Deg2Rad;

        mRadianSlideLimit = (mCharCtrl.slopeLimit + 0.1f) * Mathf.Deg2Rad;
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
        mRadianSlideLimit = (mCharCtrl.slopeLimit + 0.1f) * Mathf.Deg2Rad;
        mRayCheckDistance = mCharCtrl.height * 0.5f + mCharCtrl.radius;
#endif

        InputManager input = Main.instance.input;

        float dt = Time.fixedDeltaTime;

        if(mRopeEnabled) {
            Vector2 ropeSPos = rope.startPosition;
            Vector2 pos = transform.position;
            float len = ropeDistance;

            mOmega = Mathf.Clamp(
                mOmega + mass * Physics.gravity.y * Mathf.Sin(mTheta) * dt / len - drag * mOmega + input.GetAxis(0, InputAction.DirX) * mRadianSpeed, 
                -mRadianMaxSpeed, mRadianMaxSpeed);

            mTheta += mOmega * dt;
            
            //note: theta relative to y-axis, where 0 = up vector
            Vector2 dPos = new Vector2((ropeSPos.x + Mathf.Sin(mTheta) * len) - pos.x, (ropeSPos.y - Mathf.Cos(mTheta) * len) - pos.y);

            mCollFlags = mCharCtrl.Move(dPos);

            transform.up = ropeSPos - pos;
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
            else {
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
        //Debug.Log("fuck you");
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        mLastContactPoint = hit.point;

        //if(hit.gameObject.name == "thing")
            //Debug.Log("fuck you");
        if(mRopeEnabled) {
            float r = ropeDistance;
            Vector2 n = hit.normal;
            float dot = Vector2.Dot(n, Vector2.up);
            float theta = Mathf.Acos(dot);

            //TODO: config scalar?
            mOmega = 2.0f* Mathf.Abs(mOmega) * Mathf.Sin(theta) / r;
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
