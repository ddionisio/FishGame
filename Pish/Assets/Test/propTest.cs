using UnityEngine;
using System.Collections;

public class propTest : MonoBehaviour {

    public int a = 0;
    public float b = 0;
    public double c = 0;
    public long d = 0;

    public int aa = 0;

    public int aaa { get { return aa; } set { aa = value; } }

    public Color clr;
    public Rect r;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void Yoink(int af) {
        Debug.Log("yoink: " + af);
    }

    public void FuckityFuck(Vector3 a, long bbb, string[] sexes) {
        Debug.Log("fuck you: " + bbb + " and " + 1);
    }
}
