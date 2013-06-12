using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    [Tooltip("Use this for objects when activating an event, e.g. npc")]
    public class SpecialActionTriggerListen : FSMActionComponentBase<SpecialActionTrigger> {
        [RequiredField]
        public FsmEvent onTriggerEvent;

        public override void Reset() {
            base.Reset();

            onTriggerEvent = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            mComp.actionCallback += OnAction;
        }

        // Code that runs when exiting the state.
        public override void OnExit() {
            mComp.actionCallback -= OnAction;
        }

        void OnAction() {
            Fsm.Event(onTriggerEvent);

            Finish();
        }
    }
}
