using UnityEngine;
using System.Collections;

public class RopeController : MonoBehaviour {
    public GameObject ropeActive;

    public float maxRopeLength;

    private float mRopeLength;

    public float ropeLength {
        get { return mRopeLength; }

        set {
            if(mRopeLength != value) {
                //TODO: take into account clipped ropes to cap length
                mRopeLength = value;
                Vector3 s = ropeActive.transform.localScale;
                s.y = mRopeLength;
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
            return ropeT.position + ropeT.up * mRopeLength;
        }
    }

    public void Activate(Vector2 startPos) {
        gameObject.SetActive(true);

        //generate first rope and setup
    }

    public void Deactivate() {
        gameObject.SetActive(false);

        //clean up all ropes
    }

    public void Split(Vector2 pos) {
        //generate a new rope, freeze last one
    }

    void Awake() {
        //temp
        mRopeLength = ropeActive.transform.localScale.y;
    }
        
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
