using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class EntitySpawnTrackRegister : FSMActionComponentBase<EntitySpawnTracker> {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmGameObject entity;

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.Register(entity.Value.GetComponent<EntityBase>());

            Finish();
        }

    }
}
