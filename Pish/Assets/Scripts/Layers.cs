using UnityEngine;
using System.Collections;

public class Layers : MonoBehaviour {
    public static int fish = 0;
    public static int collect = 0;
    public static int player = 0;
    public static int spike = 0;
    public static int playerTrigger = 0;
    public static int specialTrigger = 0;

    void Awake() {
        fish = LayerMask.NameToLayer("Fish");
        collect = LayerMask.NameToLayer("Collect");
        player = LayerMask.NameToLayer("Player");
        spike = LayerMask.NameToLayer("Spike");
        playerTrigger = LayerMask.NameToLayer("PlayerTrigger");
        specialTrigger = LayerMask.NameToLayer("SpecialTrigger");
    }
}
