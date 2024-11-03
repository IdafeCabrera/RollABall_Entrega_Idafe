using UnityEngine;
using System.Collections;

public class ZoneEventArgs : System.EventArgs
{
    public BehaviorZone.BehaviorType ZoneType { get; private set; }
    public Vector3 ZonePosition { get; private set; }
    public GameObject Trigger { get; private set; }

    public ZoneEventArgs(BehaviorZone.BehaviorType type, Vector3 position, GameObject trigger)
    {
        ZoneType = type;
        ZonePosition = position;
        Trigger = trigger;
    }
}