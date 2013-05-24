using UnityEngine;
using System.Collections;

public class CollectibleEntity : EntityBase {
    private Collectible mCollectible;

    public Collectible collectible {
        get { return mCollectible; }
    }

    protected override void OnDespawned() {
        mCollectible.collectFlagged = false;

        base.OnDespawned();
    }

    protected override void Awake() {
        base.Awake();

        mCollectible = GetComponent<Collectible>();
        mCollectible.collectedCallback += Release;
    }
}
