using UnityEngine;
using System.Collections;

public class PlayerSensor : SensorCheckSphereSingle<PlayerController> {
    public delegate void OnChange(PlayerController u);

    public LayerMask visibilityBlock;

    public event OnChange addCallback;
    public event OnChange removeCallback;

    protected override bool UnitVerify(PlayerController u) {
        //check if nothing is blocking the unit
        Vector2 pos = transform.position;
        Vector2 playerPos = u.transform.position;
        Vector2 dir = playerPos - pos;
        float dist = dir.magnitude;

        if(dist > 0.0f) {
            RaycastHit hit;
            dir /= dist;
            return !Physics.Raycast(pos, dir, out hit, dist, visibilityBlock);
        }

        return true;
    }

    protected override void UnitAdded(PlayerController u) {
        if(addCallback != null)
            addCallback(u);
    }

    protected override void UnitRemoved(PlayerController u) {
        if(removeCallback != null)
            removeCallback(u);
    }

    protected override void UnitUpdate() {
    }

    protected override void OnDestroy() {
        addCallback = null;
        removeCallback = null;

        base.OnDestroy();
    }
}
