using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField, Range(0.0f, 50.0f)] private float speed;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
}
