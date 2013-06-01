using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerSetScore : FSMActionComponentBase<Player> {
        [RequiredField]
        public FsmFloat val;

        public override void Reset() {
            base.Reset();

            val = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.score = val.Value;

            Finish();
        }
    }

}
