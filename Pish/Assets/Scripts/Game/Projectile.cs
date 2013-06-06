using UnityEngine;
using System.Collections;

public class Projectile : EntityBase {
    public const int StateSpawning = 0;
    public const int StateNormal = 1;
    public const int StateEnd = 2;

    public enum ContactType {
        None,
        End,
        Stop,
        Bounce
    }

    public delegate void GenericCall(Projectile proj);

    public ContactType contactType;

    public bool applyDirToUp;

    public float initialSpeed;
    public float accel;
    public float maxSpeed;

    public float lifeSpan;

    public float radius = 0.1f;

    public LayerMask collideMask;

    public event GenericCall contactCallback; //called during contact

    private bool mReleaseOnEnd = true;
    private Vector2 mStartDir;
    private Vector2 mDir;
    private float mSpeed;
    private RaycastHit mHit;

    public static Projectile Create(string aGroup, string aType, Vector3 aStart, Vector2 aDir) {
        Projectile proj = EntityBase.Spawn<Projectile>(aGroup, aType, aStart);
        if(proj != null) {
            proj.mStartDir = aDir;
        }

        return proj;
    }

    /// <summary>
    /// If true, projectile calls Release on StateEnd
    /// </summary>
    public bool releaseOnEnd {
        get { return mReleaseOnEnd; }
        set { mReleaseOnEnd = value; }
    }
    
    public Vector2 dir {
        get { return mDir; }
    }

    public float speed {
        get { return mSpeed; }
    }

    public RaycastHit lastHit {
        get { return mHit; }
    }

    public override void SpawnFinish() {
        base.SpawnFinish();

        mSpeed = initialSpeed;
        mDir = mStartDir;

        if(lifeSpan > 0.0f)
            Invoke("OnDeath", lifeSpan);

        if(applyDirToUp)
            InvokeRepeating("OnUpUpdate", 0.1f, 0.1f);

        state = StateNormal;
    }

    protected override void SpawnStart() {
        base.SpawnStart();

        state = StateSpawning;
    }

    protected override void StateChanged() {
        switch(state) {
            case StateEnd:
                CancelInvoke("OnUpUpdate");

                //explode?

                if(mReleaseOnEnd) {
                    state = StateInvalid;
                    Release();
                }
                break;
        }
    }

    protected override void OnDestroy() {
        contactCallback = null;

        base.OnDestroy();
    }

    protected override void OnDespawned() {
        CancelInvoke("OnDeath");

        base.OnDespawned();
    }

    void FixedUpdate() {
        switch(state) {
            case StateNormal:
                Vector3 pos = transform.position;

                float dt = Time.fixedDeltaTime;

                //
                mSpeed = Mathf.Clamp(mSpeed + accel * dt, -maxSpeed, maxSpeed);

                float dist = mSpeed * dt;

                bool isHit = Physics.SphereCast(pos, radius, mDir, out mHit, dist, collideMask);

                if(isHit) {
                    pos.x = mHit.point.x + mHit.normal.x * radius;
                    pos.y = mHit.point.y + mHit.normal.y * radius;

                    transform.position = pos;

                    if(contactCallback != null)
                        contactCallback(this);

                    switch(contactType) {
                        case ContactType.None:
                            break;

                        case ContactType.Bounce:
                            mDir = M8.MathUtil.Reflect(mDir, mHit.normal);
                            break;

                        case ContactType.Stop:
                            mSpeed = 0.0f;
                            break;

                        case ContactType.End:
                            state = StateEnd;
                            break;
                    }
                }
                else {
                    pos.x += mDir.x * dist;
                    pos.y += mDir.y * dist;

                    transform.position = pos;
                }
                break;

            case StateEnd:
                if(mReleaseOnEnd) {
                    state = StateInvalid;
                    Release();
                }
                break;
        }
    }

    void OnUpUpdate() {
        transform.up = mDir;
    }

    void OnDrawGizmos() {
        if(radius > 0.0f) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    void OnDeath() {
        state = StateEnd;
    }
}
