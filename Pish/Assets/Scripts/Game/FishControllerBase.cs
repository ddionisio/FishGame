using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishControllerBase : MonoBehaviour {
    public enum MoveMode {
        Fall,
        Idle,
        Path,
        Chase,

        NumModes
    }

    public delegate void OnMoveModeChanged(FishControllerBase ctrl);

    public PlayerSensor playerSensor;

    public bool waypointPingPing = false;

    public event OnMoveModeChanged moveModeChangedCallback;

    private MoveMode mCurMoveMode = MoveMode.NumModes;
    private MoveMode mPrevMoveMode = MoveMode.NumModes;

    private string mCurWaypoint;
    private bool mCurWaypointReverse = false;
    private List<Transform> mCurWaypointList;
    private int mCurWaypointInd;

    public string waypoint {
        get { return mCurWaypoint; }
        set {
            mCurWaypoint = value;

            //hm...
            if(mCurMoveMode == MoveMode.Chase) {
                if(!string.IsNullOrEmpty(value)) {
                    mPrevMoveMode = MoveMode.Path;
                }
                else {
                    mPrevMoveMode = MoveMode.Idle;
                }
            }
            else {
                if(!string.IsNullOrEmpty(value)) {
                    curMoveMode = MoveMode.Path;
                }
                else {
                    curMoveMode = MoveMode.Idle;
                }
            }
        }
    }

    public Transform currentWaypoint {
        get { return mCurWaypointList[mCurWaypointInd]; }
    }

    public MoveMode prevMoveMode {
        get { return mPrevMoveMode; }
    }

    public MoveMode curMoveMode {
        get { return mCurMoveMode; }
        set {
            if(mCurMoveMode != value) {
                mPrevMoveMode = mCurMoveMode;
                mCurMoveMode = value;

                if(!string.IsNullOrEmpty(mCurWaypoint))
                    mCurWaypointList = WaypointManager.instance.GetWaypoints(mCurWaypoint);

                //Debug.Log(name + "mode change: " + mCurMoveMode);

                OnMoveModeChange();

                if(moveModeChangedCallback != null)
                    moveModeChangedCallback(this);
            }
        }
    }

    public virtual void SpawnStart() {
    }

    public virtual void Follow(Transform t) {
    }

    public virtual void SetWanderData(Vector3 origin) {
    }

    public virtual void SetWanderData(Vector3 origin, float radius) {
    }

    public virtual void OnPlayerContact(PlayerController pc, ControllerColliderHit hit) {
    }

    public virtual void RefreshAnimation() {
    }
    
    protected virtual void OnMoveModeChange() {
    }

    protected virtual void OnGotoPath(Transform t) {
    }

    protected virtual void OnDestroy() {
        moveModeChangedCallback = null;
    }

    protected virtual bool IsPathDone() {
        return false;
    }
        
    // Use this for initialization
    protected virtual void Start() {

    }

    protected virtual void Update() {
        switch(mCurMoveMode) {
            case MoveMode.Path:
                if(IsPathDone()) {
                    WaypointNext();
                }
                break;
        }
    }

    void WaypointNext() {
        if(mCurWaypointReverse) {
            mCurWaypointInd--;
            if(mCurWaypointInd == -1) {
                if(waypointPingPing) {
                    mCurWaypointInd = 0;
                    mCurWaypointReverse = false;
                }
                else {
                    mCurWaypointInd = mCurWaypointList.Count - 1;
                }
            }
        }
        else {
            mCurWaypointInd++;
            if(mCurWaypointInd == mCurWaypointList.Count) {
                if(waypointPingPing) {
                    mCurWaypointInd = mCurWaypointList.Count - 1;
                    mCurWaypointReverse = true;
                }
                else {
                    mCurWaypointInd = 0;
                }
            }
        }

        OnGotoPath(mCurWaypointList[mCurWaypointInd]);
    }

    protected void WaypointReverse() {
        mCurWaypointReverse = !mCurWaypointReverse;
        WaypointNext();
    }

    protected void WaypointSetToNearestIndex() {
        mCurWaypointInd = 0;

        Vector2 pos = transform.position;
        float smallestSqMag = float.MaxValue;
        for(int i = 0; i < mCurWaypointList.Count; i++) {
            Vector2 wp = mCurWaypointList[i].position;
            float sqMag = (pos - wp).sqrMagnitude;
            if(sqMag < smallestSqMag) {
                mCurWaypointInd = i;
                smallestSqMag = sqMag;
            }
        }

        mCurWaypointReverse = false;
    }
}
