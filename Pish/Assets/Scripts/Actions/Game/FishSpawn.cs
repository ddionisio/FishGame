using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishSpawn : FSMActionComponentBase<FishSpawner> {
        public FsmInt groupIndex;

        public override void Reset() {
            base.Reset();

            groupIndex = null;
        }
    
        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            if(!groupIndex.IsNone)
                mComp.chooserIndex = groupIndex.Value;

            mComp.Spawn();

            Finish();
        }

    }
}
