using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour {
    public enum State {
        boost,
        fall,
        idle,
        jump,
        move,
        roll,
        slide,
        stunned,
        swingL1,
        swingL2,
        swingMid,
        swingR1,
        swingR2,

        swingL1Boost,
        swingL2Boost,
        swingMidBoost,
        swingR1Boost,
        swingR2Boost
    }

    public enum Mode {
        Normal,
        Spin
    }

    public tk2dSpriteAnimator anim;
    public State[] swingStates;
    public State[] swingBoostStates;
    public ParticleSystem particle;

    public float spinSpeed;
    public float revertUpDelay = 0.5f;

    private tk2dSpriteAnimationClip[] mStateIds;

    private Mode mMode = Mode.Normal;
    private State mCurState;

    private bool mRevertUp = false;
    private Vector2 mLastUp;
    private float mCurRevertUpTime;
    
    public bool isFacingLeft {
        get { return anim.Sprite.FlipX; }
        set {
            if(anim.Sprite.FlipX != value) {
                anim.Sprite.FlipX = value;
            }
        }
    }

    public Mode mode {
        get { return mMode; }
        set {
            mMode = value;
        }
    }

    public State state {
        get { return mCurState; }
        set {
            if(mCurState != value) {
                mCurState = value;
                anim.Play(mStateIds[(int)mCurState]);
            }
        }
    }

    public bool particleEnable {
        get { return particle.isPlaying; }
        set {
            if(particle.isPlaying != value) {
                if(value) {
                    particle.Play();
                }
                else {
                    particle.Stop(false);
                }
            }
        }
    }

    public void RevertUpCancel() {
        mRevertUp = false;
    }

    public void RevertUp() {
        mRevertUp = true;
        mLastUp = transform.up;
        mCurRevertUpTime = 0.0f;
    }

    void Awake() {
        mStateIds = M8.tk2dUtil.GetSpriteClips(anim, typeof(State));
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        switch(mMode) {
            case Mode.Normal:
                if(mRevertUp) {
                    mCurRevertUpTime += Time.deltaTime;
                    if(mCurRevertUpTime < revertUpDelay) {
                        Vector3 up =
                            new Vector3(
                                M8.Ease.In(mCurRevertUpTime, revertUpDelay, mLastUp.x, -mLastUp.x),
                                M8.Ease.In(mCurRevertUpTime, revertUpDelay, mLastUp.y, 1.0f - mLastUp.y),
                                0.0f);

                        transform.up = up;
                    }
                    else {
                        mRevertUp = false;
                        transform.up = Vector3.up;
                    }
                }
                break;

            case Mode.Spin:
                Vector3 rot = transform.eulerAngles;
                rot.z += anim.Sprite.FlipX ? spinSpeed * Time.deltaTime : -spinSpeed * Time.deltaTime;
                transform.eulerAngles = rot;
                break;
        }
    }
}
