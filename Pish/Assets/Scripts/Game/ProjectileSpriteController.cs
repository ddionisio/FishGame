using UnityEngine;
using System.Collections;

public class ProjectileSpriteController : MonoBehaviour {
    public enum AnimState {
        normal,
        destroy
    }

    public tk2dSpriteAnimator anim;

    private Projectile mProjectile;
    private tk2dSpriteAnimationClip[] mAnimIds;

    void OnDestroy() {
        if(anim != null)
            anim.AnimationCompleted -= AnimationCompleteDelegate;
    }

    void Awake() {
        if(anim == null) {
            anim = GetComponentInChildren<tk2dSpriteAnimator>();
        }

        mAnimIds = M8.tk2dUtil.GetSpriteClips(anim, typeof(AnimState));

        anim.AnimationCompleted += AnimationCompleteDelegate;

        mProjectile = GetComponent<Projectile>();
        mProjectile.setStateCallback += OnSetState;
    }

    void OnSetState(EntityBase ent, int state) {
        switch(state) {
            case Projectile.StateNormal:
                anim.Play(mAnimIds[(int)AnimState.normal]);
                break;

            case Projectile.StateEnd:
                anim.Play(mAnimIds[(int)AnimState.destroy]);
                break;
        }
    }

    void AnimationCompleteDelegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip) {
        if(clip == mAnimIds[(int)AnimState.destroy])
            mProjectile.Release();
    }
}
