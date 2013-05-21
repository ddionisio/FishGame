using UnityEngine;
using System.Collections;

public class FishSensor : SensorCheckSphere<Fish> {
    public float zOfs = 0.01f;

    private Fish mNearestFish = null;

    public Fish nearestFish {
        get {
            return mNearestFish;
        }
    }

    protected override bool UnitVerify(Fish unit) {
        return unit.state == Fish.State.Normal;
    }

    protected override void UnitAdded(Fish unit) {
    }

    protected override void UnitRemoved(Fish unit) {
    }

    protected override void UnitUpdate() {
        float nearestSq = float.MaxValue;
        Fish nearestFish = null;

        Vector3 pos = transform.position;

        foreach(Fish fish in items) {
            if(fish != null && UnitVerify(fish)) {
                Vector3 fpos = fish.transform.position;
                Vector3 dpos = fpos - pos;
                float lenSq = dpos.sqrMagnitude;
                if(lenSq < nearestSq) {
                    nearestFish = fish;
                    nearestSq = lenSq;
                }
            }
        }

        //there should at least be one
        if(nearestFish != null) {
            if(nearestFish != mNearestFish) {
                ClearNearestFish();

                mNearestFish = nearestFish;
                mNearestFish.reticleEnabled = true;
            }
        }
        else {
            ClearNearestFish();
        }
    }

    protected override void OnDisable() {
        base.OnDisable();

        ClearNearestFish();
    }

    void Awake() {
    }

    // Use this for initialization
    void Start() {

    }

    private void ClearNearestFish() {
        if(mNearestFish != null) {
            mNearestFish.reticleEnabled = false;

            mNearestFish = null;
        }
    }
}