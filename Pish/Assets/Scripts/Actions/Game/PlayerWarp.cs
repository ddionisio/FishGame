using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerWarp : FSMActionComponentBase<Player> {
        public FsmBool isOut;
        public FsmBool includeCollector;
        public FsmEvent doneEvent;

        public override void Reset() {
            base.Reset();

            isOut = false;
            includeCollector = true;
            doneEvent = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.warpDoneCallback += WarpEnd;
            mComp.Warp(includeCollector.Value, isOut.Value);
        }

        public override void OnExit() {
            mComp.warpDoneCallback -= WarpEnd;

            base.OnExit();
        }

        void WarpEnd(Player p) {
            mComp.warpDoneCallback -= WarpEnd;

            if(!FsmEvent.IsNullOrEmpty(doneEvent))
                Fsm.Event(doneEvent);

            Finish();
        }
    }
}
