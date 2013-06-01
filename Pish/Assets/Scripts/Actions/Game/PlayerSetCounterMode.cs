using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerSetCounterMode : FSMActionComponentBase<Player> {
        public Player.CounterMode mode;

        [Tooltip("For time mode")]
        public FsmFloat countdownMax;

        public override void Reset() {
            base.Reset();

            mode = Player.CounterMode.None;
            countdownMax = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            switch(mode) {
                case Player.CounterMode.Countdown:
                    if(!countdownMax.IsNone)
                        mComp.countdownMax = countdownMax.Value;
                    break;
            }

            mComp.counterMode = mode;

            Finish();
        }

    }
}
