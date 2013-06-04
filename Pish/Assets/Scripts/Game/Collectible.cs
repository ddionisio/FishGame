using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour {
    public delegate void OnCollected();

    public enum Type {
        Fish,
        Energy,
        Collect,
        Rescue,

        NumTypes
    }

    public Type type = Type.NumTypes;
    public int ivalue;
    public float fvalue;
    public string svalue;

    public GameObject flaggedMarker;

    public event OnCollected collectedCallback;

    private bool mCollectFlagged = false;

    /// <summary>
    /// Determine if this is sent to be collected
    /// </summary>
    public bool collectFlagged {
        get { return mCollectFlagged; }
        set {
            mCollectFlagged = value;

            if(flaggedMarker != null)
                flaggedMarker.SetActive(value); 
        }
    }

    /// <summary>
    /// Called after a collector collects this
    /// </summary>
    public void Collected() {
        if(collectedCallback != null)
            collectedCallback();
    }

    void OnDestroy() {
        collectedCallback = null;
    }

    void Awake() {
        mCollectFlagged = false;

        if(flaggedMarker != null)
            flaggedMarker.SetActive(false);
    }
}
