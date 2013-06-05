using UnityEngine;
using System.Collections;

public class ProjectileSpriteController : MonoBehaviour {
    public enum AnimState {
        normal,
        destroy
    }

    public tk2dAnimatedSprite anim;

    private Projectile mProjectile;
    private int[] mAnimIds;

    void OnDestroy() {
        if(anim != null)
            anim.animationCompleteDelegate -= AnimationCompleteDelegate;
    }

    void Awake() {
        if(anim == null) {
            anim = GetComponentInChildren<tk2dAnimatedSprite>();
        }

        mAnimIds = M8.tk2dUtil.GenerateSpriteIds(anim, typeof(AnimState));

        anim.animationCompleteDelegate += AnimationCompleteDelegate;

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

    void AnimationCompleteDelegate(tk2dAnimatedSprite sprite, int clipId) {
        if(clipId == mAnimIds[(int)AnimState.destroy])
            mProjectile.Release();
    }
}
