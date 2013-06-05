using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public tk2dAnimatedSprite body;
    public GameObject openActive; //activated if open
    public GameObject triggerActive; //this should auto-disable
    public Transform spawnPoint; //position to spawn

    public void Triggered() {
        if(triggerActive != null)
            triggerActive.SetActive(true);
    }

    public void SetOpen(bool open) {
        if(openActive != null)
            openActive.SetActive(open);

        body.Play(open ? "open" : "close");
    }

    void Awake() {
        if(openActive != null)
            openActive.SetActive(false);

        body.Play("close");
    }
}
