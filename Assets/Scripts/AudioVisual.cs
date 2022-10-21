using UnityEngine;
using UnityEngine.UI;

public class AudioVisual : MonoBehaviour
{

    /******* PUBLIC VARIABLES *******/
    [Header("Object Info")]
    [Tooltip("Defines how long the object lives before being deleted")]
    public float Lifetime;

    /******* PRIVATE VARIABLES *******/
    private GameObject _audioVisualFocusButtonGameObject;
    private Image _FocusButtonImage;

    private void Awake() {
        _audioVisualFocusButtonGameObject = this.gameObject.transform.GetChild(0).gameObject;
        _FocusButtonImage = _audioVisualFocusButtonGameObject.GetComponent<Image>();
    }

    private void FixedUpdate() {
        Lifetime -= Time.fixedDeltaTime;
        if (Lifetime <= 0f) {
            Destroy(this.gameObject);
        }
    }
}