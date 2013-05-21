using UnityEngine;
using System.Collections;

public class CollectibleSensor : SensorCheckSphere<Collectible> {

    protected override bool UnitVerify(Collectible unit) {
        return unit.type != Collectible.Type.NumTypes;
    }

    protected override void UnitAdded(Collectible unit) {
    }

    protected override void UnitRemoved(Collectible unit) {
    }
}
