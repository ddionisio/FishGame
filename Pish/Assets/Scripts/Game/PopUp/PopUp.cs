using UnityEngine;
using System.Collections;

public class PopUp : MonoBehaviour {
    public enum Icon {
        Battery
    }

    public string textPoolRef;
    public string[] iconPoolRefs; //base it on enum

    public float speedMin;
    public float speedMax;
    public float angleRange; //from up vector
    public float delay;

    private static PopUp mInstance = null;

    private PoolController mPool;

    public static PopUp instance { get { return mInstance; } }

    public void SpawnIcon(Vector3 pos, Icon icon) {
        Transform spawn = mPool.Spawn(iconPoolRefs[(int)icon], null, null, null);
        spawn.position = pos;

        PopUpItemBase popup = spawn.GetComponent<PopUpItemBase>();
        popup.Init("", GenerateDir(), Random.Range(speedMin, speedMax), delay);
    }

    public void SpawnText(Vector3 pos, string text) {
        Transform spawn = mPool.Spawn(textPoolRef, null, null, null);
        spawn.position = pos;

        PopUpItemBase popup = spawn.GetComponent<PopUpItemBase>();
        popup.Init(text, GenerateDir(), Random.Range(speedMin, speedMax), delay);
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mPool = GetComponent<PoolController>();
        }
    }

    Vector2 GenerateDir() {
        float angle = Random.Range(-angleRange, angleRange);
        return M8.MathUtil.Rotate(Vector2.up, angle * Mathf.Deg2Rad);
    }
}
