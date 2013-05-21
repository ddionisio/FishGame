using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour {
    public enum Type {
        Fish,
        Energy,

        NumTypes
    }

    public Type type = Type.NumTypes;
    public int ivalue;
    public float fvalue;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
