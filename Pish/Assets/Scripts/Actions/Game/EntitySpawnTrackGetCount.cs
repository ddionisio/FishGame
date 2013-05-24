using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class EntitySpawnTrackGetCount : FSMActionComponentBase<EntitySpawnTracker> {

        [UIHint(UIHint.Variable)]
        public FsmInt output;

        public FsmBool everyFrame;

        public override void Reset() {
            base.Reset();

            output = null;
            everyFrame = false;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            if(!output.IsNone)
                output.Value = mComp.count;

            if(!everyFrame.Value)
                Finish();
        }

        // Code that runs every frame.
        public override void OnUpdate() {
            if(!output.IsNone)
                output.Value = mComp.count;
        }

    }
}
