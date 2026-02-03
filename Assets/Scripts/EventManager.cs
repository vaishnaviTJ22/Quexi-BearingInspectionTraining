using System;
using UnityEditor;
using UnityEngine;

public class EventManager
{
    public static Action<bool> UpdateMenuUIActiveState;
    public static Action<GameObject> UpdateSelectedBearing;
}