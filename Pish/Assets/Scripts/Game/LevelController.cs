using UnityEngine;
using System.Collections;

/// <summary>
/// Global settings for level.
/// </summary>
public class LevelController : MonoBehaviour {
    public float spikeHurtAmount;
    public float spikeSpeedBounce;

    private static LevelController mInstance = null;

    public static LevelController instance { get { return mInstance; } }

    /// <summary>
    /// Returns the new velocity
    /// </summary>
    public Vector2 SpikeBounceOff(ControllerColliderHit hit, Vector2 vel) {
        Vector2 dir = hit.normal;

        float velMag = vel.magnitude;
        if(velMag > 0.0f) {
            dir = -hit.moveDirection;//vel / velMag;
            //dir = M8.MathUtil.Reflect(dir, hit.normal);
        }

        return dir * spikeSpeedBounce;
    }

    void OnDestroy() {
        if(mInstance == this) {
            mInstance = null;
        }
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;
        }
    }

    // Use this for initialization
    void Start() {

    }
}
