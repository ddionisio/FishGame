using UnityEngine;
using System.Collections;

public abstract class PopUpItemBase : MonoBehaviour {
    private PoolDataController mPoolCtrl;

    private Vector2 mCurVel;
    private float mCurTime;
    private float mDelay;

    public virtual void Init(string text, Vector2 dir, float speed, float delay) {

        mDelay = delay;
        mCurVel = dir * speed;
        mCurTime = 0.0f;
    }

    void Awake() {
        mPoolCtrl = GetComponent<PoolDataController>();
    }

    void Update() {
        float dt = Time.deltaTime;

        mCurVel.y += Physics.gravity.y * dt;

        Vector3 pos = transform.position;
        pos.x += mCurVel.x * dt;
        pos.y += mCurVel.y * dt;

        transform.position = pos;

        mCurTime += dt;

        if(mCurTime >= mDelay) {
            PoolController.Release(mPoolCtrl.group, transform);
        }
    }
}
