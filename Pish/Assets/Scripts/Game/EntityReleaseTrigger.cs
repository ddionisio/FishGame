using UnityEngine;
using System.Collections;

public class EntityReleaseTrigger : MonoBehaviour {

    void OnTriggerEnter(Collider c) {
        EntityBase ent = c.GetComponent<EntityBase>();
        if(ent != null)
            ent.Release();
    }
}
