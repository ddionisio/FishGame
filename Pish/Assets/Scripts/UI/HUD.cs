using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
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

    public Transform[] pointerHolders;

    public GameObject comboHolder;
    public UISprite comboFill;
    public UILabel comboLabel;

    private UIWidget mEnergySliderBar;
    private List<NGUIPointAt>[] mPointers;

    //fillScale = 1.0f full fill
    public void RefreshCombo(int counter) {
        if(counter > 1) {
            comboHolder.SetActive(true);
            comboLabel.text = "x" + counter.ToString();
        }
        else {
            comboHolder.SetActive(false);
        }
    }

    public void RefreshComboFill(float fillScale) {
        comboFill.fillAmount = fillScale;
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
        fishCountLabel.text = Mathf.RoundToInt(val).ToString("D6");
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

        comboHolder.SetActive(false);

        if(boostPanelBlink != null)
            boostPanelBlink.enabled = false;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
