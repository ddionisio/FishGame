// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions {
    [ActionCategory("2D ToolKit/Sprite")]
    [Tooltip("Plays a sprite animation. \nNOTE: The Game Object must have a tk2dSpriteAnimator attached.")]
    public class Tk2dPlayAnimation : FsmStateAction {
        [RequiredField]
        [Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dSpriteAnimator component attached.")]
        [CheckForComponent(typeof(tk2dSpriteAnimator))]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        [Tooltip("The clip name to play")]
        public FsmString clipName;


        private tk2dSpriteAnimator _sprite;

        private void _getSprite() {
            GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(go == null) {
                return;
            }

            _sprite = go.GetComponent<tk2dSpriteAnimator>();
        }


        public override void Reset() {
            gameObject = null;
            clipName = null;
        }

        public override void OnEnter() {
            _getSprite();

            DoPlayAnimation();
        }

        void DoPlayAnimation() {
            if(_sprite == null) {
                LogWarning("Missing tk2dSpriteAnimator component");
                return;
            }

            if(_sprite.Playing == false) {

                _sprite.Play(clipName.Value);
            }
        }
    }

}