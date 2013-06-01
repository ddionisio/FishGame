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

    public delegate void OnMoveModeChanged(FishController ctrl);

    public bool waypointPingPing = false;
    public float waypointApproxRadius = 0.1f;

    public float fallSpeedLimit = 2.5f;

    public PlayerSensor playerSensor;

    public bool playerHitInvulnerable = false;
    public bool playerChase = false; //chase player, sensor needs to be valid
    public float playerChaseMaxSpeed = 30.0f;
    public bool playerContactStun = false; //stun player
    public float playerContactEnergy = 0.0f; //reduce player's energy
    public float playerContactPushSpeed = 0.0f;

    public event OnMoveModeChanged moveModeChangedCallback;

    private MoveMode mCurMoveMode = MoveMode.NumModes;
    private MoveMode mPrevMoveMode = MoveMode.NumModes;
    private FlockUnit mFlockUnit;
    private PlayerSensor mPlayerSensor;

    private string mCurWaypoint;
    private bool mCurWaypointReverse = false;
    private List<Transform> mCurWaypointList;
    private int mCurWaypointInd;
    private float mPrevFlockUnitMaxSpeed;

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

                Debug.Log(name + "mode change: " + mCurMoveMode);

                //new
                switch(mCurMoveMode) {
                    case MoveMode.Fall:
                        collider.enabled = true;
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = false;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = true;
                        mFlockUnit.maxSpeed = fallSpeedLimit;
                        break;

                    case MoveMode.Wander:
                        collider.enabled = true;
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = true;
                        mFlockUnit.wanderEnabled = true;
                        mFlockUnit.body.useGravity = false;
                        mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;
                        break;

                    case MoveMode.Path:
                        collider.enabled = true;
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = true;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = false;
                        mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;

                        mCurWaypointList = WaypointManager.instance.GetWaypoints(mCurWaypoint);

                        //get nearest point
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
                        
                        GotoCurrentPath();
                        break;

                    case MoveMode.Chase:
                        collider.enabled = true;
                        mFlockUnit.enabled = true;
                        mFlockUnit.groupMoveEnabled = false;
                        mFlockUnit.wanderEnabled = false;
                        mFlockUnit.body.useGravity = false;
                        mFlockUnit.maxSpeed = playerChaseMaxSpeed;
                        break;

                    case MoveMode.NumModes:
                        collider.enabled = false;
                        mFlockUnit.enabled = false;
                        mFlockUnit.maxSpeed = mPrevFlockUnitMaxSpeed;
                        break;
                }

                if(moveModeChangedCallback != null)
                    moveModeChangedCallback(this);
            }
        }
    }

    public void Follow(Transform t) {
        Debug.Log("follow: " + t.name);
        curMoveMode = MoveMode.Chase;
        flockUnit.moveTarget = t;
    }

    void OnDestroy() {
        moveModeChangedCallback = null;
    }

    void Awake() {
        mFlockUnit = GetComponent<FlockUnit>();
        mFlockUnit.enabled = false;
        mPrevFlockUnitMaxSpeed = mFlockUnit.maxSpeed;

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
                    if(mCurWaypointReverse) {
                        mCurWaypointInd--;
                        if(mCurWaypointInd == -1) {
                            if(waypointPingPing) {
                                mCurWaypointInd = 0;
                                mCurWaypointReverse = false;
                            }
                            else {
                                mCurWaypointInd = mCurWaypointList.Count-1;
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

                    GotoCurrentPath();
                }
                break;
        }
    }

    void OnPlayerSensorAdded(PlayerController unit) {
        if(playerChase) {
            Debug.Log("chase: " + unit.name);

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
