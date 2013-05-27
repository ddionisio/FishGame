using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.PlayMaker {
    [ActionCategory("Game")]
    public class PlayerSaveScore : FSMActionComponentBase<Player> {

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            GameData.Info info = GameData.instance.GetInfo(Application.loadedLevelName);
            if(info != null) {
                info.score = mComp.score;
            }

            Finish();
        }
    }
}
