using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //public Transform attachTo;
    public float attachDeadzone;
    public float moveDelay = 0.1f;

    public Camera lookCamera; //the main camera looking at player

    private Transform mAttachTo;

    private float mAttachDeadzoneSq;
    private Vector2 mLastAttachPos;
    private Vector2 mLastCheckPos;
    private Vector3 mCurVel = Vector3.zero;
    private Vector3 mDestPos;
    private Vector2 mSignDir;

    public Transform attachTo {
        get { return mAttachTo; }
        set {
            if(mAttachTo != value) {
                mAttachTo = value;

                if(mAttachTo != null) {
                    mLastAttachPos = mAttachTo.transform.position;
                    mLastCheckPos = mLastAttachPos;
                    mDestPos = mLastAttachPos;
                    transform.position = mLastAttachPos;
                    mCurVel = Vector3.zero;
                }
            }
        }
    }

    void Awake() {
        mAttachDeadzoneSq = attachDeadzone * attachDeadzone;
    }
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
#if UNITY_EDITOR
        mAttachDeadzoneSq = attachDeadzone * attachDeadzone;
#endif

        if(mAttachTo != null) {
            Vector2 apos = mAttachTo.position;
            Vector2 dpos = apos - mLastCheckPos;
            Vector2 dapos = apos - mLastAttachPos;

            if(dpos.sqrMagnitude > mAttachDeadzoneSq) {
                mLastCheckPos += dapos;

                Vector2 dposSign = new Vector2(Mathf.Sign(dapos.x), Mathf.Sign(dapos.y));
                if(mSignDir.x != dposSign.x) {
                    mSignDir.x = dposSign.x;
                    mLastCheckPos.x = apos.x;
                }
                if(mSignDir.y != dposSign.y) {
                    mSignDir.y = dposSign.y;
                    mLastCheckPos.y = apos.y;
                }
            }
                        
            mDestPos = mLastCheckPos;

            transform.position = Vector3.SmoothDamp(transform.position, mDestPos, ref mCurVel, moveDelay);

            mLastAttachPos = apos;
        }
    }
}