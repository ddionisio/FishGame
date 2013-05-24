using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishSpawnSetChooseIndex : FSMActionComponentBase<FishSpawner> {
        public FsmInt index;

        public override void Reset() {
            base.Reset();

            index = 0;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.chooserIndex = index.Value;

            Finish();
        }

    }
}
