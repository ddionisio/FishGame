using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishInventoryResource : MonoBehaviour {
    public GameObject[] fishTemplates;

    private Dictionary<string, GameObject> mFishTemplateDict;

    void Awake() {
        mFishTemplateDict = new Dictionary<string, GameObject>(fishTemplates.Length);

        foreach(GameObject fish in fishTemplates) {
            mFishTemplateDict.Add(fish.name, fish);
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
