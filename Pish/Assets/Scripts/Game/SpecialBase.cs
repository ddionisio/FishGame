using UnityEngine;
using System.Collections;

public abstract class SpecialBase : MonoBehaviour {
    public int maxCharge = 3;
    public float chargeRegenDelay;

    public bool lockGravity = false; //stop falling if true

    private int mCurCharge = 0;
    private bool mIsActing = false;

    public bool isActing {
        get { return mIsActing; }
    }

    public int curCharge {
        get { return mCurCharge; }
    }

    //call on start for each special, initializes data
    public void Init(PlayerController pc) {
        pc.stateCallback += OnStateChange;

        mCurCharge = maxCharge;

        OnInit(pc);
    }

    public bool Act(PlayerController pc) {
        //check if we can act
        if(mCurCharge > 0) {
            mCurCharge--;

            StopAllCoroutines();

            StartCoroutine(doCharge(pc));

            OnAct(pc);
                        
            mIsActing = true;

            return true;
        }

        return false;
    }

    public void ActStop(PlayerController pc) {
        if(mIsActing) {
            //cancel
            OnStop(pc);

            mIsActing = false;
        }
    }

    //only call if acting
    public void ActUpdate(PlayerController pc, float deltaTime) {
        OnUpdate(pc, deltaTime);
    }

    protected abstract void OnInit(PlayerController pc);

    protected abstract void OnAct(PlayerController pc);

    protected abstract void OnStop(PlayerController pc);

    protected abstract void OnUpdate(PlayerController pc, float deltaTime);

    protected virtual void OnRecharge(PlayerController pc) {
    }

    protected virtual void OnStateChange(PlayerController pc, PlayerController.State prevState) {
    }

    private IEnumerator doCharge(PlayerController pc) {
        //update hud

        while(mCurCharge < maxCharge) {
            yield return new WaitForSeconds(chargeRegenDelay);

            mCurCharge++;

            //update hud

            OnRecharge(pc);
        }

        yield break;
    }
}
