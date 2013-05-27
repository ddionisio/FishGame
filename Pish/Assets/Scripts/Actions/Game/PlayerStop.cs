using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerStop : FSMActionComponentBase<Player> {

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.Stop();

            Finish();
        }

    }
}
