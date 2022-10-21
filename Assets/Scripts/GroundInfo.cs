using UnityEngine;

public class GroundInfo : MonoBehaviour {
    /*
        TERRAIN TYPES:
        (1) Tile
        (2) Metal
    */

    [Tooltip("The name of the general terrain type")]
    public string terrainName;

    [Space(10)]
    [Header("Audio Multipliers")]
    public float WalkNoise;
    public float SprintNoise;
    public float TiptoeNoise;
    public float JumpNoise;

    [Space(10)]
    [Header("Audio Radius Definitions")]
    public float WalkRadius;
    public float SprintRadius;
    public float TiptoeRadius;
    public float JumpRadius;
}