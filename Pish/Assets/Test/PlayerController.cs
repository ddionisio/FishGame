using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float moveSpeed;
    public float jumpSpeed;
    public float maxSpeed;

    public GameObject rigidColliderObject; //the object used for rigid body's collider

    public RopeController rope;

    public GameObject hookGun;

    private ConfigurableJoint mHookGunJoint;

    private CharacterController mCharCtrl;

    private Vector2 mCurVel;

    private bool mInputEnabled = false;

    private bool mRigidEnabled = false;

    private CollisionFlags mCollFlags = CollisionFlags.None; //updated during move

    private CapsuleCollider mRigidCollider;

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

    public bool rigidEnabled {
        get {
            return mRigidEnabled;
        }
        set {
            if(mRigidEnabled != value) {
                mRigidEnabled = value;

                if(mRigidEnabled) {
                    mCharCtrl.enabled = false;
                    rigidColliderObject.SetActive(true);
                    rigidbody.isKinematic = false;
                }
                else {
                    mCharCtrl.enabled = true;
                    rigidColliderObject.SetActive(false);
                    rigidbody.isKinematic = true;
                }
            }
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }
    
    void Awake() {
        mCharCtrl = GetComponent<CharacterController>();
        //mCharCtrl.enabled = false;

        mRigidCollider = rigidColliderObject.GetComponent<CapsuleCollider>();

        //set the properties of rigid collider to that of char collider's
        mRigidCollider.radius = mCharCtrl.radius;
        mRigidCollider.height = mCharCtrl.height;
        mRigidCollider.center = mCharCtrl.center;

        //set default to character in control
        mRigidEnabled = false;

        mCharCtrl.enabled = true;
        rigidColliderObject.SetActive(false);
        rigidbody.isKinematic = true;

        //hookgun stuff
        mHookGunJoint = hookGun.GetComponent<ConfigurableJoint>();
    }

    // Use this for initialization
    void Start() {
        inputEnabled = true;
        rigidEnabled = true;
    }
        
    // Update is called once per frame
    void Update() {
        if(mCharCtrl.enabled) {
            InputManager input = Main.instance.input;

            float dt = Time.fixedDeltaTime;

            //apply fall if we are not colliding below
            if(!mCharCtrl.isGrounded) {
                mCurVel.y += Physics.gravity.y * dt;
            }
            else {
                //move left/right
                mCurVel.x = input.GetAxis(0, InputAction.DirX) * moveSpeed;
            }

            M8.MathUtil.Limit(ref mCurVel, maxSpeed);

            mCollFlags = mCharCtrl.Move(mCurVel*dt);

            if((mCollFlags & CollisionFlags.Above) != 0) {
                if(mCurVel.y > 0.0f)
                    mCurVel.y = 0.0f;
            }
        }
    }

    void FixedUpdate() {
        if(hookGun.activeSelf) {
            mHookGunJoint.targetPosition = rope.endPosition;
        }
    }

    void OnAction(InputManager.Info data) {
        if(mCharCtrl.enabled) {
            //jump
            if(mCharCtrl.isGrounded) {
                mCurVel.y = jumpSpeed;
            }
            else {
                //rope action
            }
        }
        else if(hookGun.activeSelf) {
            //rope deactivate
        }
    }

    void OnCollisionEnter(Collision coll) {
        //Debug.Log("fuck you");
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        //if(hit.gameObject.name == "thing")
            //Debug.Log("fuck you");
    }
}
