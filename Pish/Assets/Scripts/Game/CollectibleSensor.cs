using UnityEngine;
using System.Collections;

public class CollectibleSensor : SensorCheckSphere<Collectible> {
    public Collector collector;

    protected override bool UnitVerify(Collectible unit) {
        return unit.type != Collectible.Type.NumTypes && !unit.collectFlagged;
    }

    protected override void UnitAdded(Collectible unit) {
        unit.collectFlagged = true;
        collector.AddToQueue(unit);
    }

    protected override void UnitRemoved(Collectible unit) {
    }
}
