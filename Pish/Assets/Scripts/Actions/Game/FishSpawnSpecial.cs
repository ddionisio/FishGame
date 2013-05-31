using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class FishSpawnSpecial : FSMActionComponentBase<FishSpawner> {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmGameObject go;

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.SpawnSpecialFish(go.Value.transform);

            Finish();
        }
    }

}
