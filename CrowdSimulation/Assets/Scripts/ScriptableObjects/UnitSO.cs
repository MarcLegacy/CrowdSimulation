using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class UnitSO : ScriptableObject
{
    public string unitName = "";
    public float speed = 10f;
    public float acceleration = 1f;
    public float senseDistance = 1f;

    [Header("Movement Forces")]
    public MovementForcesInfo alignment = new MovementForcesInfo();
    public MovementForcesInfo cohesion = new MovementForcesInfo();
    public MovementForcesInfo separation = new MovementForcesInfo();
    public MovementForcesInfo obstacleAvoidance = new MovementForcesInfo();
}
