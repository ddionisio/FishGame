using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishControlSetWaypoint : FSMActionComponentBase<FishController> {
        public FsmString waypoint;

        public override void Reset() {
            base.Reset();

            waypoint = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.waypoint = waypoint.Value;

            Finish();
        }


    }
}