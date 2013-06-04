using UnityEngine;
using System.Collections;

public class FishingResultController : BaseResultController {
    public PoolController pool;
    public Transform fishHolder;

    public float fishTossAngleRange;
    public float fishSpeedMin;
    public float fishSpeedMax;
    public float fishTorqueMin;
    public float fishTorqueMax;

    public float fishTossStartDelay;
    public float fishTossDelay;

    protected override void OnDestroy() {
        if(FishInventory.instance != null)
            FishInventory.instance.items.Clear();

        base.OnDestroy();
    }

    void Awake() {
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        StartCoroutine(SaladToss());
    }

    void OnInputContinue(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            Main.instance.sceneManager.LoadScene(LevelSelectController.lastLevelSelect);
        }
    }

    void OnFishSpawn(EntityBase ent) {
        Fish f = ent as Fish;
        f.state = Fish.StateStunned;
        //f.controller.flockUnit.enabled = false;

        Rigidbody body = f.rigidbody;

        Vector2 vel = M8.MathUtil.Rotate(Vector2.up, Random.Range(-fishTossAngleRange * 0.5f, fishTossAngleRange * 0.5f) * Mathf.Deg2Rad);
        vel *= Random.Range(fishSpeedMin, fishSpeedMax);

        body.AddForce(vel, ForceMode.VelocityChange);
        body.AddTorque(0.0f, 0.0f, Random.Range(fishTorqueMin, fishTorqueMax));

        ent.spawnCallback -= OnFishSpawn;
    }

    IEnumerator SaladToss() {
        yield return new WaitForSeconds(fishTossStartDelay);

#if false
        //test
        for(int i = 0; i < 10; i++) {
            Transform t = pool.Spawn("fish1", null, fishHolder, null);
            EntityBase ent = t.GetComponent<EntityBase>();
            ent.spawnCallback += OnFishSpawn;

            yield return new WaitForSeconds(fishTossDelay);
        }
#else
        FishInventory inventory = FishInventory.instance;

        foreach(FishInventory.Item itm in inventory.items) {
            Transform t = pool.Spawn(itm.type, null, fishHolder, null);
            EntityBase ent = t.GetComponent<EntityBase>();
            ent.spawnCallback += OnFishSpawn;
            
            yield return new WaitForSeconds(fishTossDelay);
        }
#endif

        yield break;
    }
}
