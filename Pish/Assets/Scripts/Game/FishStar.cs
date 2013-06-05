using UnityEngine;
using System.Collections;

public class FishStar : Fish {
    public float expireDelay = 10.0f;
    public float switchPointDelay = 3.0f;

    public TweenScale scaleTweener;

    private Transform mWanderPoints;

    private UITweener.Method mTweenPrevMethod;

    protected override void SpawnStart() {
        base.SpawnStart();

        if(mWanderPoints != null) {
            StartCoroutine(DoIt());
        }
    }

    protected override void OnDespawned() {
        scaleTweener.method = mTweenPrevMethod;

        base.OnDespawned();
    }

    protected override void Awake() {
        base.Awake();

        GameObject wanderPtsGO = GameObject.FindGameObjectWithTag("StarfishPoints");
        if(wanderPtsGO != null) {
            mWanderPoints = wanderPtsGO.transform;
        }

        mTweenPrevMethod = scaleTweener.method;
    }

    void OnFinishedTween(UITweener tween) {
        scaleTweener.onFinished -= OnFinishedTween;

        Release();
    }

    Vector3 GetWanderPoint() {
        if(mWanderPoints == null)
            return Vector3.zero;

        int ind = Random.Range(0, mWanderPoints.GetChildCount());

        return mWanderPoints.GetChild(ind).position;
    }

    IEnumerator DoIt() {
        yield return new WaitForFixedUpdate();
                
        controller.curMoveMode = FishController.MoveMode.Idle;
        controller.SetWanderData(GetWanderPoint());
        state = StateNormal;

        float startTime = Time.fixedTime;
        float curTime = 0.0f;

        while(Time.fixedTime - startTime < expireDelay) {
            curTime += Time.fixedDeltaTime;

            if(curTime >= switchPointDelay) {
                //change wander point
                controller.SetWanderData(GetWanderPoint());

                curTime = 0.0f;
            }

            yield return new WaitForFixedUpdate();
        }

        state = StateInvalid;

        scaleTweener.enabled = true;
        scaleTweener.method = UITweener.Method.BounceOut;
        scaleTweener.Reset();

        scaleTweener.onFinished += OnFinishedTween;
    }
}
