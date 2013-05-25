using UnityEngine;
using System.Collections;

public class Layers : MonoBehaviour {
    public static int fish = 0;
    public static int collect = 0;
    public static int player = 0;

    void Awake() {
        fish = LayerMask.NameToLayer("Fish");
        collect = LayerMask.NameToLayer("Collect");
        player = LayerMask.NameToLayer("Player");
    }
}
