using UnityEngine;
using System.Collections;

public class FishFireOnSight : MonoBehaviour {

    public float damage = 1.0f;

    public tk2dSpriteAnimator anim;
    public FishControllerBase controller;

    public string shootAnimRef = "shoot";

    public string projectileSpawnGroup;
    public string projectileSpawnType;

    public float projectileReloadDelay = 1.0f;

    public LayerMask obstructMask;

    private enum State {
        Scanning,
        Shooting,
        ShootWait
    }

    private State mState = State.Scanning;
    private tk2dSpriteAnimationClip mShootAnimId;
    private PlayerController mPlayerController;
    private Projectile mProj;

    void OnDestroy() {
    }

    void Awake() {
        mShootAnimId = anim.GetClipByName(shootAnimRef);
    }

    void OnTriggerStay(Collider col) {
        if(mState == State.Scanning) {
            mPlayerController = col.GetComponent<PlayerController>();
            if(mPlayerController != null) {
                bool shoot = false;

                //make sure it is not obstructed
                //obstructMask
                Vector2 dir = mPlayerController.transform.position - transform.position;
                float dist = dir.magnitude;

                if(dist > 0.0f) {
                    dir /= dist;
                    shoot = !Physics.Raycast(transform.position, dir, dist, obstructMask);
                }

                if(shoot) {
                    mProj = Projectile.Create(projectileSpawnGroup, projectileSpawnType, transform.position, transform.up);
                    if(mProj != null) {
                        mProj.releaseCallback += OnProjectileRelease;
                        mProj.contactCallback += OnProjContact;

                        controller.curMoveMode = FishControllerBase.MoveMode.Idle;

                        anim.Play(mShootAnimId);

                        mState = State.Shooting;
                    }
                    else {
                        mPlayerController = null;
                    }
                }
                else {
                    mPlayerController = null;
                }
            }
        }
    }

    void OnProjContact(Projectile proj) {
        if(mPlayerController != null && proj.lastHit.collider.gameObject == mPlayerController.gameObject) {
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

        if(projectileReloadDelay > 0.0f)
            StartCoroutine(Reload());
        else
            mState = State.Scanning;
    }

    IEnumerator Reload() {
        mState = State.ShootWait;

        yield return new WaitForSeconds(projectileReloadDelay);

        mState = State.Scanning;
    }
}
