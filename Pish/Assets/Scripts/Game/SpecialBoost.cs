using UnityEngine;
using System.Collections;

public class SpecialBoost : SpecialBase {
    public float speed;
    public float ropingRevolution;
    public float speedAngleLimit = 89.0f;
    
    public float delay;

    private float mCurTime;
    private float mRopingDir;

    protected override void OnInit(PlayerController pc) {
    }

    protected override void OnAct(PlayerController pc) {
        pc.animator.mode = PlayerAnimator.Mode.Normal;

        if(pc.state == PlayerController.State.Normal) {
            Vector2 dir;

            if(pc.curInputAxis != Vector2.zero) {
                pc.curVelocity = pc.curInputAxis.normalized * speed;
            }
            else {
                //check if there's a target nearby
                Fish fishNearby = pc.fishSensor.nearestFish;
                if(fishNearby != null) {
                    dir = fishNearby.transform.position - pc.transform.position;
                    dir.Normalize();

                    pc.curVelocity = dir * speed;
                }
                else {
                    //otherwise, just boost on current direction
                    if(pc.charCtrl.isGrounded) {
                        dir = pc.curVelocity.x == 0.0f ? Vector2.up : new Vector2(Mathf.Sign(pc.curVelocity.x), 0.0f);
                    }
                    else {
                        dir = pc.curVelocity.normalized;
                        M8.MathUtil.DirCap(Vector2.up, ref dir, speedAngleLimit);
                    }

                    pc.curVelocity = dir * speed;
                }
            }

            pc.animator.state = PlayerAnimator.State.boost;
        }
        else {
            if(pc.curInputAxis.x != 0.0f) {
                mRopingDir = Mathf.Sign(pc.curInputAxis.x);
            }
            else {
                mRopingDir = Mathf.Sign(pc.curAngleVelocity);
            }
        }

        mCurTime = 0.0f;

        pc.fishSensor.gameObject.SetActive(false);
    }

    protected override void OnStop(PlayerController pc) {
        ActivateFishSensor(pc);

        pc.animator.mode = PlayerAnimator.Mode.Normal;
        if(pc.state != PlayerController.State.Roping) {
            pc.animator.RevertUp();
        }
    }

    protected override void OnUpdate(PlayerController pc, float deltaTime) {
        mCurTime += deltaTime;
        if(mCurTime >= delay) {
            ActStop(pc);
        }
        else {
            if(pc.state == PlayerController.State.Roping) {
                pc.curAngleVelocity = pc.curAngleVelocity + (mRopingDir * ropingRevolution) / (2.0f * Mathf.PI * pc.ropeDistance);
            }
            else {
                pc.animator.transform.up = pc.curVelocity;
            }
        }
    }

    protected override void OnRecharge(PlayerController pc) {
        ActivateFishSensor(pc);
    }

    protected override void OnStateChange(PlayerController pc, PlayerController.State prevState) {
        ActivateFishSensor(pc);
    }

    private void ActivateFishSensor(PlayerController pc) {
        switch(pc.state) {
            case PlayerController.State.Normal:
                pc.fishSensor.gameObject.SetActive(!isActing && curCharge > 0);
                break;

            case PlayerController.State.Stunned:
            case PlayerController.State.RopeShoot:
            case PlayerController.State.Roping:
                pc.fishSensor.gameObject.SetActive(false);
                break;
        }
    }
}
