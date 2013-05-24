using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//helper to keep track of entities currently spawned
public class EntitySpawnTracker : MonoBehaviour {

    private HashSet<EntityBase> mEnts = new HashSet<EntityBase>();

    public int count {
        get { return mEnts.Count; }
    }

    public void Register(EntityBase ent) {
        if(ent != null) {
            mEnts.Add(ent);
            ent.releaseCallback += OnEntityRelease;
        }
    }

    void OnDestroy() {
        foreach(EntityBase ent in mEnts) {
            if(ent != null)
                ent.releaseCallback -= OnEntityRelease;
        }
    }

    void OnEntityRelease(EntityBase ent) {
        ent.releaseCallback -= OnEntityRelease;

        mEnts.Remove(ent);
    }
}
