using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerRespawn : FSMActionComponentBase<Player> {
        public override void Reset() {
            base.Reset();
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.Respawn();

            Finish();
        }

    }
}
