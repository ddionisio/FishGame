using UnityEngine;
using System.Collections;

public class FishStats : MonoBehaviour {
    public delegate void OnChange(FishStats stats);

    public int maxHit;

    public event OnChange changeCallback;

    private int mCurHit;

    public int curHit {
        get { return mCurHit; }
        set {
            if(mCurHit != value) {
                mCurHit = Mathf.Clamp(value, 0, maxHit);

                if(changeCallback != null)
                    changeCallback(this);
            }
        }
    }

    public void ResetStats() {
        mCurHit = maxHit;
    }

    void OnDestroy() {
        changeCallback = null;
    }

    void Awake() {
    }

    // Use this for initialization
    void Start() {
        ResetStats();
    }
}
