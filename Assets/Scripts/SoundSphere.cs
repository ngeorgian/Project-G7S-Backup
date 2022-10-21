using UnityEngine;

public class SoundSphere : MonoBehaviour
{
    [Header("Audio Info")]
    [Tooltip("Reference to the GameObject of the thing making the noise")]
    public GameObject SoundEmitterGameObject;
    [Tooltip("References the GroundInfo object used to create a sound (if applicable)")]
    public GroundInfo GroundInfo;
    [Tooltip("References the volume a sound was played at")]
    public float volume;
    [Tooltip("The color that the sound shows up as in the audiovisual for the player")]
    public Color audioVisualColor;
    [Tooltip("Flag for making the audio visual focusable while in audio mode")]
    public bool audioFocus;
}