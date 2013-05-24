using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishControlSetWanderPosition : FSMActionComponentBase<FishController> {

        public FsmVector3 position;

        public override void Reset() {
            base.Reset();

            position = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            if(position.IsNone) {
                mComp.flockUnit.wanderOriginLock = false;
            }
            else {
                mComp.flockUnit.wanderOrigin = position.Value;
                mComp.flockUnit.wanderOriginLock = true;
            }

            Finish();
        }


    }
}
