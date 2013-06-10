using UnityEngine;
using System.Collections;

public class FishFireTimer : MonoBehaviour {

    public float damage = 1.0f;
    public float delay = 1.0f;

    public tk2dSpriteAnimator anim;
    public FishControllerBase controller;

    public string shootAnimRef = "shoot";

    public string projectileSpawnGroup;
    public string projectileSpawnType;

    private enum State {
        Shooting,
        ShootWait
    }

    private State mState = State.ShootWait;
    private tk2dSpriteAnimationClip mShootAnimId;
    private PlayerController mPlayerController;
    private Projectile mProj;

    private bool mStarted = false;

    void OnDestroy() {
    }

    void OnEnable() {
        if(mStarted)
            StartCoroutine(ShootDelay());
    }

    void Awake() {
        mShootAnimId = anim.GetClipByName(shootAnimRef);
    }

    void Start() {
        mStarted = true;
        StartCoroutine(ShootDelay());
    }

    IEnumerator ShootDelay() {
        WaitForSeconds waitDelay = new WaitForSeconds(delay);
        WaitForFixedUpdate waitUpdate = new WaitForFixedUpdate();

        //wait for controller to be ready
        while(controller.curMoveMode == FishControllerBase.MoveMode.NumModes)
            yield return waitUpdate;

        while(true) {
            switch(mState) {
                case State.ShootWait:
                    yield return waitDelay;

                    //shoot
                    mProj = Projectile.Create(projectileSpawnGroup, projectileSpawnType, transform.position, transform.up);
                    if(mProj != null) {
                        mProj.releaseCallback += OnProjectileRelease;
                        mProj.contactCallback += OnProjContact;

                        controller.curMoveMode = FishControllerBase.MoveMode.Idle;

                        anim.Play(mShootAnimId);

                        mState = State.Shooting;
                    }
                    break;

                case State.Shooting:
                    yield return waitUpdate;
                    break;
            }
        }

    }

    void OnProjContact(Projectile proj) {
        bool doHit = false;

        if(mPlayerController == null) {
            mPlayerController = proj.lastHit.collider.GetComponent<PlayerController>();
            doHit = mPlayerController != null;
        }
        else {
            doHit = proj.lastHit.collider.gameObject == mPlayerController.gameObject;
        }

        if(doHit) {
            if(mPlayerController.state != PlayerController.State.Stunned) {
                mPlayerController.Hurt(damage);
                mPlayerController.state = PlayerController.State.Stunned;
            }
        }
    }

    void OnProjectileRelease(EntityBase ent) {
        mProj.releaseCallback -= OnProjectileRelease;
        mProj.contactCallback -= OnProjContact;
        mProj = null;

        mPlayerController = null;

        if(controller.prevMoveMode != FishControllerBase.MoveMode.NumModes)
            controller.curMoveMode = controller.prevMoveMode;

        controller.RefreshAnimation();

        mState = State.ShootWait;
    }
}
