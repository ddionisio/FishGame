using UnityEngine;
using System.Collections;

public class UILevelItem : MonoBehaviour {
    public string dialog;
    public string level;

    private UIEventListener mEventListener;
    private ModalLevelDialogBase mDialog;

    public UIEventListener eventListener {
        get { return mEventListener; }
    }

    void Awake() {
        mEventListener = GetComponent<UIEventListener>();
    }

    void Start() {
        if(!string.IsNullOrEmpty(dialog)) {
            mDialog = UIModalManager.instance.ModalGetController<ModalLevelDialogBase>(dialog);
        }
    }

    void OnClick() {
        if(mDialog != null) {
            mDialog.Init(level);
            UIModalManager.instance.ModalOpen(dialog);
        }
    }
}
