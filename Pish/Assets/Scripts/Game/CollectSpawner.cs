using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectSpawner : MonoBehaviour {
    [System.Serializable]
    public class SpawnData {
        public Transform[] pts;
    }

    public string poolGroup;
    public string poolType;

    public SpawnData[] spawnPoints;

    private EntitySpawnTracker mTracker;
    private int mCurCount = 0;

    void Awake() {
        mTracker = GetComponent<EntitySpawnTracker>();
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if(mTracker.count == 0) {
            SpawnData data = spawnPoints[mCurCount];

            foreach(Transform pt in data.pts) {
                Transform t = PoolController.Spawn(poolGroup, poolType, null, null, null);

                t.position = pt.position;

                mTracker.Register(t.GetComponent<EntityBase>());
            }

            mCurCount++;
            if(mCurCount == spawnPoints.Length)
                mCurCount = 0;
        }
    }
}
