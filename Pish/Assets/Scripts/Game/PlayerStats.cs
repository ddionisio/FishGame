using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour {
    public delegate void OnChange(PlayerStats stats);

    public float batteryStart;
    public float batteryMax;
    public float batteryDrain; //per seconds

    public event OnChange changeCallback;

    private float mCurBattery;

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
        mCurBattery = batteryStart;

        if(changeCallback != null) {
            changeCallback(this);
        }
    }

    public void Run() {
        InvokeRepeating("BatteryDrain", 1.0f, 1.0f);
    }

    public void Stop() {
        CancelInvoke("BatteryDrain");
    }

    void OnDestroy() {
        changeCallback = null;
    }

    void Awake() {
        ResetStats();
    }
    
    void BatteryDrain() {
        if(batteryDrain != 0.0f)
            curBattery -= batteryDrain;
    }
}
