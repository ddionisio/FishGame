using UnityEngine;
using System.Collections;

public class ModalOptions : UIController {
    public UISlider musicSlider;
    public UISlider soundSlider;

    protected override void OnActive(bool active) {
        if(active) {
            musicSlider.sliderValue = Main.instance.userSettings.musicVolume;
            musicSlider.onValueChange = OnMusicChange;

            soundSlider.sliderValue = Main.instance.userSettings.soundVolume;
            soundSlider.onValueChange = OnSoundChange;
        }
        else {
            musicSlider.onValueChange = null;
            soundSlider.onValueChange = null;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    public void OnMusicChange(float val) {
        Main.instance.userSettings.musicVolume = val;
    }

    public void OnSoundChange(float val) {
        Main.instance.userSettings.soundVolume = val;
    }
}
