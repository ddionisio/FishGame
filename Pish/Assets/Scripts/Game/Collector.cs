using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collector : MonoBehaviour {
    public delegate void OnCollectReached(Collectible collect);

    public const int collectCapacity = 20;

    public Transform ownerAttach;

    public float seekDelay;
    public float returnDelay;
    public float dirDelay;
    public float pathApproxRadius;
    public float followDirAngleLimit = 45.0f;

    public event OnCollectReached collectReachedCallback;

    private Vector3 mCurVel = Vector3.zero;
    private Vector3 mCurDirVel = Vector3.zero;

    private Transform mTrans;

    private Queue<Collectible> mCollects;
    
    public void AddToQueue(Collectible collect) {
        collect.collectFlagged = true;
        mCollects.Enqueue(collect);
    }

    void OnDestroy() {
        collectReachedCallback = null;
    }

    void Awake() {
        mCollects = new Queue<Collectible>(collectCapacity);

        mTrans = transform;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Vector3 pos = mTrans.position;
        Vector3 dest;
        Vector2 dir;

        float moveDelay;

        if(mCollects.Count > 0) {
            Collectible collect = mCollects.Peek();
            dest = collect.transform.position;

            dir = dest - pos;
            float dist = dir.magnitude;
            if(dist > 0.0f) {
                dir /= dist;

                //collect
                if(dist <= pathApproxRadius) {
                    if(collectReachedCallback != null)
                        collectReachedCallback(collect);

                    collect.Collected();

                    mCollects.Dequeue();
                }
            }
            else {
                dir = Vector2.up;
            }

            mTrans.up = dir;

            moveDelay = seekDelay;
        }
        else {
            dest = ownerAttach.position;
            dir = dest - pos;
            float dist = dir.magnitude;

            if(dist > pathApproxRadius) {
                dir /= dist;

                M8.MathUtil.DirCap(Vector2.up, ref dir, followDirAngleLimit);
            }
            else {
                dir = Vector2.up;
            }

            moveDelay = returnDelay;
        }

        mTrans.up = Vector3.SmoothDamp(mTrans.up, dir, ref mCurDirVel, dirDelay);

        mTrans.position = Vector3.SmoothDamp(pos, dest, ref mCurVel, moveDelay);
    }
}
