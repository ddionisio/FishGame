using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour, IComparer<FishSpawner.SpawnData> {
    [System.Serializable]
    public class SpawnData {
        public string type; //prefab ref
        public string group; //pool group
        public float weight = 1.0f; //chance to spawn

        private float mRange = 0.0f;

        public float range {
            get { return mRange; }
            set { mRange = value; }
        }
    }

    [System.Serializable]
    public class SpawnGroup {
        public string name;
        public SpawnData[] items;

        private float mMaxRange = 0.0f;

        public float maxRange { get { return mMaxRange; } }

        public void ComputeMaxRange() {
            mMaxRange = 0.0f;
            foreach(SpawnData dat in items) {
                mMaxRange += dat.weight;
                dat.range = mMaxRange;
            }
        }
    }

    [System.Serializable]
    public class SpawnPoint {
        public Transform[] points;
        public string waypoint;
        public float wanderRadius;

        public Color gizmoColor;
    }

    public SpawnGroup[] group;
    public SpawnPoint[] points;

    public string specialFishGroup = "fish";
    public string specialFishRef = "starfish1";
    public string specialFishSpawnTag = "StarfishPoints";
    public float specialFishMinDist = 10.0f;
            
    private SpawnData mPicker = new SpawnData();
    private int mChooserInd = 0;
    private int mPickedInd = 0;
    private int mPointInd = 0;

    private EntitySpawnTracker mSpawnTracker;

    private class SpecialFishSpawnPoint {
        public Vector2 pos;
        public float sqDist;

        public SpecialFishSpawnPoint(Transform t) {
            pos = t.position;
        }
    }

    private List<SpecialFishSpawnPoint> mSpecialFishSpawnPoints = null;

    public int chooserIndex {
        get { return mChooserInd; }
        set {
            mChooserInd = value;
            //
        }
    }

    public void SpawnSpecialFish(Transform player) {
        if(!string.IsNullOrEmpty(specialFishRef) && mSpecialFishSpawnPoints != null) {
            Transform spawn = PoolController.Spawn(specialFishGroup, specialFishRef, null, null, null);

            //sort points based on what's nearest player
            mSpecialFishSpawnPoints.Sort(
                delegate(SpecialFishSpawnPoint v1, SpecialFishSpawnPoint v2) {
                    Vector2 pos = player.position;

                    v1.sqDist = (v1.pos - pos).sqrMagnitude;
                    v2.sqDist = (v2.pos - pos).sqrMagnitude;

                    return v2.sqDist.CompareTo(v2.sqDist);
            });

            //get a point outside the min distance
            float sqMinDist = specialFishMinDist * specialFishMinDist;

            Vector2 to = player.position;

            foreach(SpecialFishSpawnPoint pt in mSpecialFishSpawnPoints) {
                if(pt.sqDist >= sqMinDist) {
                    to = pt.pos;
                    break;
                }
            }

            spawn.position = to;
        }
    }

    public void Spawn() {
        Debug.Log("spawning from group index: " + mChooserInd);

        if(points != null && points.Length > 0) {
            SpawnPoint sp = points[mPointInd];

            foreach(Transform point in sp.points) {
                mPicker.range = Random.Range(0.0f, group[mChooserInd].maxRange);

                mPickedInd = System.Array.BinarySearch(group[mChooserInd].items, mPicker, this);

                if(mPickedInd < 0) {
                    mPickedInd = Mathf.Clamp(~mPickedInd, 0, group[mChooserInd].items.Length - 1);
                }

                SpawnData dat = group[mChooserInd].items[mPickedInd];

                Transform spawn = PoolController.Spawn(dat.group, dat.type, null, point, null);

                Fish fish = spawn.GetComponent<Fish>();
                fish.controller.SetWanderData(point.position, sp.wanderRadius);
                fish.controller.waypoint = sp.waypoint;

                mSpawnTracker.Register(fish);
            }

            mPointInd++;
            if(mPointInd == points.Length)
                mPointInd = 0;
        }
        else {
            mPicker.range = Random.Range(0.0f, group[mChooserInd].maxRange);

            mPickedInd = System.Array.BinarySearch(group[mChooserInd].items, mPicker, this);

            if(mPickedInd < 0) {
                mPickedInd = Mathf.Clamp(~mPickedInd, 0, group[mChooserInd].items.Length - 1);
            }

            SpawnData dat = group[mChooserInd].items[mPickedInd];

            Transform spawn = PoolController.Spawn(dat.group, dat.type, null, transform, null);

            Fish fish = spawn.GetComponent<Fish>();
            fish.controller.SetWanderData(transform.position);
            fish.controller.waypoint = null;

            mSpawnTracker.Register(fish);
        }
    }

    void Awake() {
        foreach(SpawnGroup grp in group) {
            grp.ComputeMaxRange();
        }

        mSpawnTracker = GetComponent<EntitySpawnTracker>();

        GameObject specialFishPtsGO = GameObject.FindGameObjectWithTag(specialFishSpawnTag);
        if(specialFishPtsGO != null) {
            Transform t = specialFishPtsGO.transform;
            int numChild = t.GetChildCount();
            mSpecialFishSpawnPoints = new List<SpecialFishSpawnPoint>(numChild);

            for(int i = 0; i < numChild; i++) {
                Transform pt = t.GetChild(i);
                mSpecialFishSpawnPoints.Add(new SpecialFishSpawnPoint(pt));
            }
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public int Compare(SpawnData obj1, SpawnData obj2) {
        if(obj1 != null && obj2 != null) {
            float v = obj1.range - obj2.range;

            if(Mathf.Abs(v) <= float.Epsilon)
                return 0;
            else if(v < 0.0f)
                return -1;
            else
                return 1;
        }
        else if(obj1 == null && obj2 != null) {
            return 1;
        }
        else if(obj2 == null && obj1 != null) {
            return -1;
        }

        return 0;
    }

    void OnDrawGizmos() {
        if(points != null) {
            foreach(SpawnPoint sp in points) {


                foreach(Transform t in sp.points) {
                    Gizmos.color = sp.gizmoColor;
                    Gizmos.DrawSphere(t.position, 0.1f);

                    if(sp.wanderRadius > 0.0f) {
                        Gizmos.color *= 0.5f;
                        Gizmos.DrawWireSphere(t.position, sp.wanderRadius);
                    }
                }
            }
        }
    }
}
