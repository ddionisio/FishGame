using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishSpawn : FSMActionComponentBase<FishSpawner> {
        public override void Reset() {
            base.Reset();
        }
    
        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.Spawn();

            Finish();
        }

    }
}
