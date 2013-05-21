using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour {
    public enum State {
        None,
        Normal,
        Stunned,

        NumStates
    }

    public GameObject reticle;

    private FishStats mStats;

    private State mState = State.None;

    public FishStats stats {
        get { return mStats; }
    }
    
    public bool reticleEnabled {
        get {
            return reticle != null && reticle.activeSelf;
        }

        set {
            if(reticle != null) {
                reticle.SetActive(value);
            }
        }
    }

    public State state {
        get { return mState; }

        set {
            if(mState != value) {
                //prev

                mState = value;

                //new
                switch(mState) {
                    case State.Normal:
                        gameObject.layer = Layers.fish;
                        break;

                    case State.Stunned:
                        gameObject.layer = Layers.collect;
                        break;
                }
            }
        }
    }

    public void PlayerContact(PlayerController pc, Vector2 dir, float speed, ControllerColliderHit hit) {
        float pushSpeed;

        if(speed >= pc.fishHitSpeedCriteria) {
            Debug.Log("fish hurt");

            stats.curHit--;

            pushSpeed = pc.fishHitPushSpeed;
        }
        else {
            pushSpeed = pc.fishContactSpeed;
        }

        rigidbody.velocity = hit.moveDirection * pushSpeed;
    }

    void Awake() {
        mStats = GetComponent<FishStats>();
        mStats.changeCallback += OnStatChange;

        reticleEnabled = false;
    }

	// Use this for initialization
	void Start () {
        state = State.Normal;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnStatChange(FishStats stats) {
        if(stats.curHit == 0) {
            state = State.Stunned;
        }
    }
}
