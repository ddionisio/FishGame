using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishController : MonoBehaviour {
    public enum MoveMode {
        Fall,
        Wander,
        Path,
        Chase,

        NumModes
    }

    public float waypointApproxRadius = 0.1f;

    public PlayerSensor playerSensor;

    public bool playerHitInvulnerable = false;
    public bool playerChase = false; //chase player, sensor needs to be valid
    public bool playerContactStun = false; //stun player
    public float playerContactEnergy = 0.0f; //reduce player's energy
    public float playerContactPushSpeed = 0.0f;

    private MoveMode mCurMoveMode = MoveMode.NumModes;
    private MoveMode mPrevMoveMode = MoveMode.NumModes;
    private FlockUnit mFlockUnit;
    private PlayerSensor mPlayerSensor;

    private string mCurWaypoint;
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
                    mPrevMoveMode = MoveMode.Wander;
                }
            }
            else {
                if(!string.IsNullOrEmpty(value)) {
                    curMoveMode = MoveMode.Path;
                }
                else {
                    curMoveMode = MoveMode.Wander;
                }
            }
        }
    }
    
    public FlockUnit flockUnit {
        get { return mFlockUnit; }
    }

    public MoveMode curMoveMode {
        get { return mCurMoveMode; }
        set {
            if(mCurMoveMode != value) {
                //prev
                mPrevMoveMode = mCurMoveMode;

                switch(mPrevMoveMode) {
                    case MoveMode.Chase:
                        mFlockUnit.moveTarget = null;
                        break;
                }

                mCurMoveMode = value;

                //new
                switch(mCurMoveMode) {
                    case MoveMode.Fall:
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = false;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = true;
                        break;

                    case MoveMode.Wander:
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = true;
                        mFlockUnit.wanderEnabled = true;
                        mFlockUnit.body.useGravity = false;
                        break;

                    case MoveMode.Path:
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = true;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = false;

                        mCurWaypointList = WaypointManager.instance.GetWaypoints(mCurWaypoint);
                        mCurWaypointInd = 0;
                        GotoCurrentPath();
                        break;

                    case MoveMode.Chase:
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = false;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = false;
                        break;

                    case MoveMode.NumModes:
                        mFlockUnit.enabled = false;
                        break;
                }
            }
        }
    }

    void Awake() {
        mFlockUnit = GetComponent<FlockUnit>();
        mFlockUnit.enabled = false;

        if(playerSensor != null) {
            playerSensor.addCallback += OnPlayerSensorAdded;
            playerSensor.removeCallback += OnPlayerSensorRemoved;
        }
    }

    // Update is called once per frame
    void Update() {
        switch(mCurMoveMode) {
            case MoveMode.Path:
                if(mFlockUnit.moveTargetDistance <= waypointApproxRadius) {
                    mCurWaypointInd++;
                    if(mCurWaypointInd == mCurWaypointList.Count)
                        mCurWaypointInd = 0;

                    GotoCurrentPath();
                }
                break;
        }
    }

    void OnPlayerSensorAdded(PlayerController unit) {
        if(playerChase) {
            if(mCurMoveMode == MoveMode.Chase) {
                mFlockUnit.moveTarget = unit.transform;
            }
            else if(mCurMoveMode != MoveMode.Fall || mCurMoveMode != MoveMode.NumModes) {
                curMoveMode = MoveMode.Chase;
                mFlockUnit.moveTarget = unit.transform;
            }
        }
    }

    void OnPlayerSensorRemoved(PlayerController unit) {
        if(mFlockUnit.moveTarget == unit.transform) {
            curMoveMode = mPrevMoveMode;
        }
    }

    private void GotoCurrentPath() {
        mFlockUnit.moveTarget = mCurWaypointList[mCurWaypointInd];
    }
}
