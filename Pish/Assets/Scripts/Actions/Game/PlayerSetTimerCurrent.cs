using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    [Tooltip("set the time value")]
    public class PlayerSetTimerCurrent : FSMActionComponentBase<Player> {
        [RequiredField]
        public FsmFloat val;

        public override void Reset() {
            base.Reset();

            val = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.hud.timerCurrent = val.Value;

            Finish();
        }

    }
}
