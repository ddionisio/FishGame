using UnityEngine;
using System.Collections;

public class CollectibleEntity : EntityBase {
    public UITweener tweenSpawn; //wait for this to spawn start?

    public bool fallEnabled;
    public float fallMaxVelocity = 50.0f;
    public LayerMask fallMask; //what to check to stop the fall

    public int hudPointerIndex = -1;
        
    private Collectible mCollectible;
    private HUD mHUD;

    private NGUIPointAt mHUDPointIndicator;

    public Collectible collectible {
        get { return mCollectible; }
    }

    protected override void OnDespawned() {
        collider.enabled = false;

        mCollectible.collectFlagged = false;

        if(tweenSpawn != null) {
            tweenSpawn.enabled = false;
        }

        if(hudPointerIndex >= 0 && mHUD != null) {
            mHUD.ReleasePointer(hudPointerIndex, mHUDPointIndicator);
            mHUDPointIndicator = null;
        }

        base.OnDespawned();
    }

    protected override void SpawnStart() {
        base.SpawnStart();

        collider.enabled = true;

        if(hudPointerIndex >= 0 && mHUD != null) {
            mHUDPointIndicator = mHUD.AllocatePointer(hudPointerIndex);
            mHUDPointIndicator.SetPOI(transform);
        }
        
        if(tweenSpawn != null) {
            tweenSpawn.enabled = true;
            tweenSpawn.Reset();
        }
    }

    public override void SpawnFinish() {
        base.SpawnFinish();

        if(fallEnabled) {
            StartCoroutine(DoFall());
        }
    }

    protected override void Awake() {
        base.Awake();

        mCollectible = GetComponent<Collectible>();
        mCollectible.collectedCallback += Release;

        if(tweenSpawn != null) {
            tweenSpawn.enabled = false;
            tweenSpawn.onFinished += OnTweenFinished;

            autoSpawnFinish = false;
        }

        if(hudPointerIndex >= 0) {
            GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
            mHUD = hudGO.GetComponent<HUD>();
        }

        collider.enabled = false;
    }

    void OnTweenFinished (UITweener tween) {
        SpawnFinish();
    }

    IEnumerator DoFall() {
        SphereCollider sc = collider != null ? collider as SphereCollider : null;
        if(sc != null) {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();

            float curVel = 0.0f;
            float radius = sc.radius;

            Vector3 pos = transform.position;
            Vector3 endPos = pos;

            yield return wait;

            RaycastHit hit;
            if(Physics.SphereCast(pos, radius, -Vector3.up, out hit, Mathf.Infinity, fallMask)) {
                endPos = hit.point + hit.normal * radius;
            }

            bool done = false;
            while(!done) {
                yield return wait;

                float dt = Time.fixedDeltaTime;

                curVel = Mathf.Clamp(curVel + Physics.gravity.y * dt, -fallMaxVelocity, 0.0f);

                pos = transform.position;

                float dy = curVel*dt;

                if(pos.y + dy <= endPos.y) {
                    pos = endPos;

                    done = true;
                }
                else {
                    pos.y += dy;
                }

                transform.position = pos;
            }
        }

        yield break;
    }
}
