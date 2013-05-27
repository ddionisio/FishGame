using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//make sure to clear out once no longer needed, before going for another fishing!
public class FishInventory : MonoBehaviour {
    public struct Item {
        public string type;
        public int ival;
        public float fval;
    }

    private static FishInventory mInstance;

    private List<Item> mItems = new List<Item>();

    public static FishInventory instance { get { return mInstance; } }

    public List<Item> items { get { return mItems; } }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            //initialize stuff
        }
    }
}
