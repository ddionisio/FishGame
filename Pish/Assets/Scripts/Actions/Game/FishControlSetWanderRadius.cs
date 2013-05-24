using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishControlSetWanderRadius : FSMActionComponentBase<FishController> {
        public FsmFloat radius;

        public override void Reset() {
            base.Reset();

            radius = 0.0f;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.flockUnit.wanderRadius = radius.Value;

            Finish();
        }

    }
}
