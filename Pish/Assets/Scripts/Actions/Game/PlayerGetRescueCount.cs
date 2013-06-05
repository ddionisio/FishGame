using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerGetRescueCount : FSMActionComponentBase<Player> {

        [RequiredField]
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

            output.Value = mComp.rescueCount;

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            output.Value = mComp.rescueCount;
        }
    }
}
