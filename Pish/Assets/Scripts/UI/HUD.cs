using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public float energyLowScale; //if energy scale [0, 1] is below this, show warning

    public UISlider energySlider;
    public UILabel energyPercentLabel;

    public Color[] energyColors; //low to high

    public GameObject energyLowLabel;
        
    public UISprite[] boostSprites;
    public string boostActiveRef;
    public string boostInactiveRef;

    public UILabel fishCountLabel;

    public Transform pointerHolder;

    private UIWidget mEnergySliderBar;
    private List<NGUIPointAt> mPointers;

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
    }

    public void RefreshFishCount(int count) {
        fishCountLabel.text = count.ToString("D3");
    }

    public NGUIPointAt AllocatePointer() {
        NGUIPointAt ret = null;

        if(mPointers != null && mPointers.Count > 0) {
            ret = mPointers[mPointers.Count - 1];
            ret.gameObject.SetActive(true);
            mPointers.RemoveAt(mPointers.Count - 1);
        }

        return ret;
    }

    public void ReleasePointer(NGUIPointAt pt) {
        if(mPointers != null && pt != null) {
            pt.SetPOI(null);
            pt.gameObject.SetActive(false);
            mPointers.Add(pt);
        }
    }

    void Awake() {
        energyLowLabel.SetActive(false);

        mEnergySliderBar = energySlider.foreground.GetComponent<UIWidget>();

        NGUIPointAt[] points = pointerHolder.GetComponentsInChildren<NGUIPointAt>(true);
        mPointers = new List<NGUIPointAt>(points.Length);
        foreach(NGUIPointAt pt in points) {
            pt.gameObject.SetActive(false);
            mPointers.Add(pt);
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
