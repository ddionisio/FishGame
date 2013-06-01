using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    [Tooltip("get the time value")]
    public class PlayerGetTimerCurrent : FSMActionComponentBase<Player> {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmFloat output;

        public FsmBool everyFrame;

        public override void Reset() {
            base.Reset();

            output = null;
            everyFrame = false;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            output.Value = mComp.hud.timerCurrent;

            if(!everyFrame.Value)
                Finish();
        }

        // Code that runs every frame.
        public override void OnUpdate() {
            output.Value = mComp.hud.timerCurrent;
        }
    }

}
