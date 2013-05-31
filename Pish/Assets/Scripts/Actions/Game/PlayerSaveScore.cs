using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerSaveScore : FSMActionComponentBase<Player> {

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            GameData.instance.SaveLevelScore(Application.loadedLevelName, mComp.score);

            Finish();
        }
    }
}
