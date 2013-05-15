using UnityEngine;
using System.Collections;

public class RopeController : MonoBehaviour {
    public GameObject hook;

    public GameObject ropeActive;

    public float maxLength;
    public float minLength;

    public float fireSpeed;
    public float expandSpeed;

    private float mCurLength;

    private bool mIsAttached;

    private float mAccumLength; //length from ropes that are split

    public bool isAttached {
        get { return mIsAttached; }
    }

    public float curLength {
        get { return mCurLength; }

        set {
            if(mCurLength != value) {
                mCurLength = Mathf.Clamp(value, minLength, maxLength - mAccumLength);

                Vector3 s = ropeActive.transform.localScale;
                s.y = mCurLength;

                //set rope display tiling correctly
            }
        }
    }

    public Vector3 startPosition {
        get {
            return ropeActive.transform.position;
        }
    }

    public Vector3 endPosition {
        get {
            Transform ropeT = ropeActive.transform;
            return ropeT.position + ropeT.up * mCurLength;
        }
    }

    public void ExtendLength(float scale, float deltaTime) {
        curLength += scale * expandSpeed * deltaTime;
    }

    //prep up for display, call before firing
    public void Activate() {
        gameObject.SetActive(true);

        //generate first rope and setup
    }

    //call while rope is being fired, endPos changes over time
    public void UpdatePosition(Vector2 startPos, Vector2 endPos) {
    }

    //once a wall is hit, this is called
    public void Attach(Vector2 endPos) {
               

        mIsAttached = true;
    }

    public void Detach() {
        gameObject.SetActive(false);

        //clean up all ropes
        mAccumLength = 0.0f;
    }

    public void Split(Vector2 pos) {
        //generate a new rope, freeze last one
    }

    void Awake() {
        //temp
        mCurLength = ropeActive.transform.localScale.y;
    }
        
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //animate rope moving to destination
    }
}
