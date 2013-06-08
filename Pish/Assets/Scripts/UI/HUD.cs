using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public enum TimerMode {
        None,
        Increase,
        Decrease
    }

    public float energyLowScale; //if energy scale [0, 1] is below this, show warning
        
    public UISlider energySlider;
    public UILabel energyPercentLabel;

    public Color[] energyColors; //low to high

    public GameObject energyLowLabel;

    public GOActiveBlink boostPanelBlink;
    public UISprite[] boostSprites;
    public string boostActiveRef;
    public string boostInactiveRef;

    public UILabel fishCountLabel;
    public UITweener fishCountTween;
    public float fishCountSpeed;

    public Transform[] pointerHolders;

    public GameObject counterHolder;
    public UISprite counterFill;
    public UILabel counterLabel;

    public UILabel timerLabel;

    public Transform rescueItemHolder;

    private UIWidget mEnergySliderBar;
    private List<NGUIPointAt>[] mPointers;

    private float mTimerCurrent = 0.0f;
    private TimerMode mTimerMode = TimerMode.None;

    private HUDRescueItem[] mRescueItems;

    private float mFishCountCur;
    private int mFishCountRound;
    private float mFishCountDest;

    public float timerCurrent {
        get { return mTimerCurrent; }
        set { mTimerCurrent = value; }
    }

    public TimerMode timerMode {
        get { return mTimerMode; }
        set {
            if(mTimerMode != value) {
                if(mTimerMode == TimerMode.None && value != TimerMode.None) {
                    StartCoroutine(DoTimer());
                }
                else if(value == TimerMode.None) {
                    StopCoroutine("DoTimer");
                }

                mTimerMode = value;
            }
        }
    }

    public void RescueInit(GameObject itemTemplate, int count) {
        if(count > 0) {
            mRescueItems = new HUDRescueItem[count];

            for(int i = 0; i < count; i++) {
                GameObject go = Instantiate(itemTemplate) as GameObject;
                Transform t = go.transform;
                t.parent = rescueItemHolder;
                t.localScale = Vector3.one;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                mRescueItems[i] = go.GetComponent<HUDRescueItem>();
                mRescueItems[i].icon.gameObject.SetActive(false);
            }

            M8.NGUIExtUtil.LayoutRefresh(rescueItemHolder);
        }
        else if(mRescueItems != null) {
            foreach(HUDRescueItem itm in mRescueItems) {
                Destroy(itm.gameObject);
            }

            mRescueItems = null;
        }
    }

    public void RescueRefresh(int count) {
        if(mRescueItems != null) {
            count = Mathf.Clamp(count, 0, mRescueItems.Length);

            int ind = 0;
            for(; ind < count; ind++) {
                mRescueItems[ind].icon.gameObject.SetActive(true);
            }

            for(; ind < mRescueItems.Length; ind++) {
                mRescueItems[ind].icon.gameObject.SetActive(false);
            }
        }
    }

    public void TimerStart() {
    }

    //fillScale = 1.0f full fill
    public void RefreshCounter(string format, int counter) {
        counterLabel.text = string.Format(format, counter);
    }

    public void RefreshCounterFill(float fillScale) {
        counterFill.fillAmount = fillScale;
    }

    public void RefreshPlayerStats(PlayerStats stats) {
        float energyScale = stats.curBattery/stats.batteryMax;

        Color clr = M8.ColorUtil.Lerp(energyColors, energyScale);

        energySlider.sliderValue = energyScale;

        energyPercentLabel.color = clr;
        energyPercentLabel.text = string.Format("{0}%", Mathf.RoundToInt(energyScale * 100.0f));

        mEnergySliderBar.color = clr;

        energyLowLabel.SetActive(energyScale <= energyLowScale);
    }

    public void RefreshBoost(SpecialBase special) {
        int ind = 0;

        for(; ind < special.curCharge; ind++) {
            boostSprites[ind].spriteName = boostActiveRef;
        }

        for(; ind < boostSprites.Length; ind++) {
            boostSprites[ind].spriteName = boostInactiveRef;
        }

        if(boostPanelBlink != null)
            boostPanelBlink.enabled = special.curCharge <= 0;
        
    }

    public void RefreshFishScore(float val) {
        mFishCountDest = val;
        
        if(fishCountTween != null) {
            fishCountTween.Reset();
            fishCountTween.enabled = true;
        }
    }

    public NGUIPointAt AllocatePointer(int ind) {
        NGUIPointAt ret = null;

        if(mPointers != null) {
            List<NGUIPointAt> pts = mPointers[ind];

            if(pts != null && pts.Count > 0) {
                ret = pts[pts.Count - 1];
                ret.gameObject.SetActive(true);
                pts.RemoveAt(pts.Count - 1);
            }
        }

        return ret;
    }

    public void ReleasePointer(int ind, NGUIPointAt pt) {
         if(mPointers != null) {
            List<NGUIPointAt> pts = mPointers[ind];

            if(pts != null) {
                pt.SetPOI(null);
                pt.gameObject.SetActive(false);
                pts.Add(pt);
            }
        }
    }

    void Awake() {
        energyLowLabel.SetActive(false);

        if(fishCountTween != null) {
            fishCountTween.enabled = false;
        }

        mEnergySliderBar = energySlider.foreground.GetComponent<UIWidget>();

        if(pointerHolders != null && pointerHolders.Length > 0) {
            mPointers = new List<NGUIPointAt>[pointerHolders.Length];
            for(int i = 0; i < pointerHolders.Length; i++) {
                Transform p = pointerHolders[i];
                NGUIPointAt[] points = p.GetComponentsInChildren<NGUIPointAt>(true);
                mPointers[i] = new List<NGUIPointAt>(points.Length);
                foreach(NGUIPointAt pt in points) {
                    pt.gameObject.SetActive(false);
                    mPointers[i].Add(pt);
                }
            }
        }

        counterHolder.SetActive(false);

        if(boostPanelBlink != null)
            boostPanelBlink.enabled = false;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if(mFishCountCur != mFishCountDest) {
            bool isDec = mFishCountCur > mFishCountDest;

            if(isDec) {
                mFishCountCur -= fishCountSpeed * Time.deltaTime;
                if(mFishCountCur < mFishCountDest) {
                    mFishCountCur = mFishCountDest;
                }
            }
            else {
                mFishCountCur += fishCountSpeed * Time.deltaTime;
                if(mFishCountCur > mFishCountDest) {
                    mFishCountCur = mFishCountDest;
                }
            }

            int v = Mathf.RoundToInt(mFishCountCur);
            if(v != mFishCountRound) {
                mFishCountRound = v;
                fishCountLabel.text = mFishCountRound.ToString("D6");
            }
        }
    }

    void RefreshTimer() {
        if(timerLabel.gameObject.activeInHierarchy) {
            int centi = Mathf.RoundToInt(mTimerCurrent * 100.0f);
            int seconds = Mathf.RoundToInt(mTimerCurrent);
            int minutes = seconds / 60;

            timerLabel.text = string.Format("{0:D2}:{1:D2}:{2:D2}", minutes % 60, seconds % 60, centi % 100);
        }
    }

    IEnumerator DoTimer() {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while(true) {
            yield return wait;

            switch(mTimerMode) {
                case TimerMode.None:
                    yield break;

                case TimerMode.Decrease:
                    mTimerCurrent -= Time.fixedDeltaTime;
                    break;

                case TimerMode.Increase:
                    mTimerCurrent += Time.fixedDeltaTime;
                    break;
            }

            RefreshTimer();
        }
    }
}
