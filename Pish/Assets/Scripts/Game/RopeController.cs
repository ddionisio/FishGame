using UnityEngine;
using System.Collections;

public class RopeController : MonoBehaviour {
    public GameObject ropeTemplate;
    public int ropeMax = 20;

    public GameObject hook;

    public float minLengthClip; //minimum length required to clip

    public float maxLength;
    public float minLength;

    public float fireSpeed;
    public float expandSpeed;

    public Material ropeMaterial;
    public float ropeTileScale;

    private float mCurLength = 0.0f;

    private bool mIsAttached = false;

    private float mAccumLength = 0.0f; //length from ropes that are split

    private int mNumRopeActive = 0;
    private GameObject[] mRopes;

    private Vector3 mRopeOriginalScale;

    private Vector2 mRopeTextureSize;

    public GameObject ropeLastActive {
        get {
            if(mNumRopeActive > 0) {
                return mRopes[mNumRopeActive - 1];
            }

            return null;
        }
    }

    public bool isAttached {
        get { return mIsAttached; }
    }

    public float curLength {
        get { return mCurLength; }

        set {
            if(mCurLength != value) {
                Transform ropeT = ropeLastActive.transform;

                mCurLength = Mathf.Clamp(value, minLength, maxLength - mAccumLength);

                Vector3 s = ropeT.localScale;
                s.y = mCurLength;
                ropeT.localScale = s;

                //set rope display tiling correctly
            }
        }
    }

    public Vector3 startPosition {
        get {
            return ropeLastActive.transform.position;
        }
    }

    public Vector3 endPosition {
        get {
            Transform ropeT = ropeLastActive.transform;
            return ropeT.position + ropeT.up * mCurLength;
        }
    }

    //dir = towards rope start pos (which is where the hook is)
    public void ExtendLength(float scale, float deltaTime) {
        curLength = mCurLength + scale * expandSpeed * deltaTime;
    }

    //prep up for display, call before firing
    //origin = player's fire point
    //theta = angle starting from up vector
    public void Fire(Vector3 origin, Vector3 dir) {
        mIsAttached = false;

        gameObject.SetActive(true);

        GameObject rope = RopeNew();

        Vector3 pos = new Vector3(origin.x + dir.x * minLength, origin.y + dir.y * minLength, rope.transform.position.z);

        rope.transform.up = -dir;
        rope.transform.position = pos;
        curLength = minLength;

        hook.SetActive(true);
        hook.transform.position = new Vector3(pos.x, pos.y, hook.transform.position.z);
        hook.transform.up = dir;
    }

    //call while rope is being fired
    //returns true when we are done
    //will attach if we collide with something
    public bool UpdateFire(Vector3 origin, Vector3 dir, float deltaTime, int collisionMask) {
        bool maxReached = false;

        float newLen = mCurLength + fireSpeed * deltaTime;

        maxReached = newLen >= maxLength;
        if(maxReached) {
            newLen = maxLength;
        }

        Vector3 ropeStartPos;

        Transform ropeT = ropeLastActive.transform;
                
        RaycastHit hit;
        mIsAttached = Physics.Raycast(origin, dir, out hit, newLen, collisionMask);
        if(mIsAttached) {
            curLength = (origin - hit.point).magnitude;
            ropeStartPos = new Vector3(hit.point.x, hit.point.y, ropeT.position.z);
        }
        else {
            curLength = newLen;
            ropeStartPos = new Vector3(origin.x + dir.x * mCurLength, origin.y + dir.y * mCurLength, ropeT.position.z);
        }

        ropeT.position = ropeStartPos;
        hook.transform.position = new Vector3(ropeStartPos.x, ropeStartPos.y, hook.transform.position.z);

        RopeUpdateFX(ropeLastActive);

        return maxReached || mIsAttached;
    }

    //update while attached, will clip rope depending on collision from start to origin
    public void UpdateAttach(Vector3 origin, int collisionMask) {
        Transform ropeT = ropeLastActive.transform;
        Vector3 pos = ropeT.position;

        Vector2 dir = origin - pos;
        dir.Normalize();

        ropeT.up = dir;

        if(mNumRopeActive == 1)
            hook.transform.up = -dir;

        //Vector3 castPoint = new Vector3(pos.x + dir.x * minLengthClip, pos.y + dir.y * minLengthClip, pos.z);
        if(mCurLength > minLengthClip) {
            RaycastHit hit;
            if(Physics.Raycast(origin, -dir, out hit, mCurLength - minLengthClip, collisionMask)) {
                //set clip length, acquire new rope, set new rope length

                Vector2 dpos = hit.point - pos;
                float dist = dpos.magnitude;

                GameObject newRope = RopeNew();
                if(newRope != null) {
                    mAccumLength += dist;

                    //set the first half
                    ropeT.up = dpos;
                    Vector3 ropeScale = ropeT.localScale;
                    ropeScale.y = dist;
                    ropeT.localScale = ropeScale;

                    RopeUpdateFX(ropeT.gameObject);

                    //second half
                    Transform newRopeT = newRope.transform;

                    Vector3 newPos = new Vector3(hit.point.x, hit.point.y, newRopeT.position.z);
                    Vector2 newDPos = origin - newPos;
                    float newDist = newDPos.magnitude;

                    newRopeT.position = newPos;
                    newRopeT.up = newDPos;
                    curLength = newDist;
                }
            }
            else if(mNumRopeActive > 1) {
                //check if we can unclip from last rope
                GameObject lastRope = mRopes[mNumRopeActive - 2];
                Transform lastRopeT = lastRope.transform;
                Vector3 lastPos = lastRopeT.position;

                Vector2 dirToLastRope = lastPos - origin;
                float lastDist = dirToLastRope.magnitude;
                dirToLastRope /= lastDist;

                if(!Physics.Raycast(origin, dirToLastRope, out hit, lastDist - minLengthClip, collisionMask)) {
                    //unclip
                    ropeT.gameObject.SetActive(false);

                    mNumRopeActive--;

                    lastRope.SetActive(true);

                    mAccumLength -= lastRopeT.localScale.y;

                    lastRopeT.up = -dirToLastRope;
                    curLength = lastDist;
                }
            }
        }

        RopeUpdateFX(ropeLastActive);
    }

    public void Detach() {
        mIsAttached = false;

        RopeRemoveAll();

        gameObject.SetActive(false);

        //clean up all ropes
        mAccumLength = 0.0f;
    }

    void Awake() {
        mRopeOriginalScale = ropeTemplate.transform.localScale;
                
        mRopes = new GameObject[ropeMax];
        for(int i = 0; i < ropeMax; i++) {
            GameObject newObj = GameObject.Instantiate(ropeTemplate) as GameObject;
            newObj.renderer.material = ropeMaterial;// Object.Instantiate(ropeMaterial) as Material;
            newObj.transform.parent = transform;
            newObj.transform.localPosition = Vector3.zero;
            newObj.SetActive(false);
            mRopes[i] = newObj;
        }

        mRopeTextureSize = new Vector2(ropeMaterial.mainTexture.width, ropeMaterial.mainTexture.height);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //animate rope moving to destination
    }

    private void RopeUpdateFX(GameObject rope) {
        Material mat = rope.renderer.material;
        float sY = rope.transform.localScale.y;
        mat.SetFloat("tileY", sY / ropeTileScale);
    }

    private GameObject RopeNew() {
        if(mNumRopeActive < mRopes.Length) {
            GameObject obj = mRopes[mNumRopeActive];
            obj.SetActive(true);

            Transform t = obj.transform;
            t.localPosition = Vector3.zero;
            t.localScale = mRopeOriginalScale;
            t.localRotation = Quaternion.identity;

            mNumRopeActive++;

            return obj;
        }

        return null;
    }

    private void RopeRemoveAll() {
        for(int i = 0; i < mNumRopeActive; i++) {
            mRopes[i].SetActive(false);
        }

        mNumRopeActive = 0;
    }
}
