using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerSetTimerMode : FSMActionComponentBase<Player> {
        public HUD.TimerMode mode;

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.hud.timerMode = mode;

            Finish();
        }
    }

}
