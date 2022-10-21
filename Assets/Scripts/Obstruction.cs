using UnityEngine;

public class Obstruction : MonoBehaviour
{
    [Header("Obstruction Info")]
    [Tooltip("The name that appears when player gets close to the obstruction in audio mode")]
    public string AudioVisualName = "";
    [Tooltip("The level of noise reduction by the obstruction -> default is 0.25")]
    public float NoiseReduction = 0.25f;
    [Tooltip("Full obstructions refers to large obstructions that heavily block sound like full walls")]
    public bool FullObstruction = false;
    [Tooltip("Reduces the max distance a sound can be heard if it goes through this obstruction")]
    public float MaxDistanceReduction = 0f;
}