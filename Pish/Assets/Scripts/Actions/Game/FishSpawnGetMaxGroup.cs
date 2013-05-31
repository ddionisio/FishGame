using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishSpawnGetMaxGroup : FSMActionComponentBase<FishSpawner> {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt output;

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            output.Value = mComp.group.Length;

            Finish();
        }
    }
}
