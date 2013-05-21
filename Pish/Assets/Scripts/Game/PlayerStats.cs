using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour {
    public delegate void OnChange(PlayerStats stats);

    public float batteryMax;
    public float batteryDrain; //per seconds

    public event OnChange changeCallback;

    private float mCurBattery;

    private bool mStarted = false;

    public float curBattery {
        get { return mCurBattery; }
        set {
            if(mCurBattery != value) {
                mCurBattery = Mathf.Clamp(value, 0.0f, batteryMax);

                if(changeCallback != null) {
                    changeCallback(this);
                }
            }
        }
    }

    public void ResetStats() {
        mCurBattery = batteryMax;
    }

    void OnDestroy() {
        changeCallback = null;
    }

    void Awake() {
        ResetStats();
    }

    void OnEnable() {
        if(mStarted) {
            Run();
        }
    }

    // Use this for initialization
    void Start() {
        mStarted = true;
        Run();
    }

    void Run() {
        InvokeRepeating("BatteryDrain", 1.0f, 1.0f);
    }

    void BatteryDrain() {
        curBattery -= batteryDrain;
    }
}
