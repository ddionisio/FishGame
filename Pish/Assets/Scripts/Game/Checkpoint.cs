using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public tk2dSpriteAnimator body;
    public GameObject openActive; //activated if open
    public GameObject triggerActive; //this should auto-disable
    public Transform spawnPoint; //position to spawn

    public SoundPlayer openSound;

    private bool mIsOpen;

    public void Triggered() {
        if(triggerActive != null)
            triggerActive.SetActive(true);
    }

    public void SetOpen(bool open) {
        if(mIsOpen != open) {
            mIsOpen = open;

            if(openActive != null)
                openActive.SetActive(open);

            body.Play(open ? "open" : "close");

            if(open && openSound != null)
                openSound.Play();
        }
    }

    void Awake() {
        mIsOpen = false;

        if(openActive != null)
            openActive.SetActive(false);

        body.Play("close");
    }
}
