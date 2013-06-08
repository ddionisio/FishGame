using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerInitRescueHUD : FSMActionComponentBase<Player> {
        [Tooltip("UI prefab for icon on HUD")]
        [RequiredField]
        public FsmGameObject template;

        [Tooltip("This is the game object that contains all the rescue entities on the level")]
        [RequiredField]
        public FsmGameObject rescueContainer;

        [UIHint(UIHint.Variable)]
        public FsmInt rescueCountOut;

        public override void Reset() {
            base.Reset();

            template = null;
            rescueContainer = null;
            rescueCountOut = null;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            int numRescue = rescueContainer.Value.transform.GetChildCount();

            Debug.Log("numRescue: " + numRescue);

            mComp.hud.RescueInit(template.Value, numRescue);

            if(!rescueCountOut.IsNone)
                rescueCountOut.Value = numRescue;

            Finish();
        }

    }
}
