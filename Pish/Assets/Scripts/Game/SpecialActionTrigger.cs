using UnityEngine;
using System.Collections;

/// <summary>
/// This is used by PlayerController for interacting with triggers when the special action is pressed.
/// </summary>
public class SpecialActionTrigger : MonoBehaviour {
    public delegate void OnAction();

    public GameObject activateGO;

    public PlayerController.State stateRequired = PlayerController.State.NumStates;
    public bool mustBeGrounded = false;

    public event OnAction actionCallback;

    private PlayerController mPlayerController;
    private bool mModalIsActive = false;

    public PlayerController playerController {
        get { return mPlayerController; }
        set {
            mPlayerController = value;

            if(mPlayerController == null && activateGO != null)
                activateGO.SetActive(false);
        }
    }

    public bool criteriaValid {
        get {
            return mPlayerController != null
                && (stateRequired == PlayerController.State.NumStates || mPlayerController.state == stateRequired)
                && (!mustBeGrounded || mPlayerController.charCtrl.isGrounded);
        }
    }

    void Awake() {
        if(activateGO != null) {
            activateGO.SetActive(false);
        }
    }

    void Update() {
        if(!mModalIsActive && mPlayerController != null && activateGO != null) {
            activateGO.SetActive(criteriaValid);
        }
    }

    void OnUIModalActive() {
        mModalIsActive = true;

        if(activateGO != null)
            activateGO.SetActive(false);
    }

    void OnUIModalInactive() {
        mModalIsActive = false;
    }

    /// <summary>
    /// Called by player controller
    /// </summary>
    public void Action() {
        if(actionCallback != null)
            actionCallback();
    }

    void OnDestroy() {
        actionCallback = null;
    }
}
