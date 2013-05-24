using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : SensorCheckSphere<FlockFilter> {
    public LayerMask obstacleCheck;

    protected override bool UnitVerify(FlockFilter unit) {
        Vector3 pos = unit.transform.position;
        Vector3 dpos = unit.transform.position - transform.position;
        float dist = dpos.magnitude;
        if(dist > 0) {
            Vector3 dir = dpos / dist;

            return !Physics.Raycast(pos, dir, dist, obstacleCheck);
        }
        else {
            return true;
        }
    }

    protected override void UnitAdded(FlockFilter unit) {
    }

    protected override void UnitRemoved(FlockFilter unit) {
    }
}
