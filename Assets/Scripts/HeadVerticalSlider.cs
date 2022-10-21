using UnityEngine;
using UnityEngine.UI;

public class HeadVerticalSlider : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Reference to Player gameobject")]
    public GameObject PlayerGameObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _controller;
    private Slider _slider;

    private void Awake() {
        _controller = PlayerGameObject.GetComponent<PlayerController>();
        _slider = GetComponent<Slider>();
    }

    private void Update() {
        // set slider value based on if player is looking more up (270 to 360) or more down (0 through 90)
        if (_controller.CinemachineCameraTarget.transform.eulerAngles.x > 270f) {
            _slider.value = 90f - (_controller.CinemachineCameraTarget.transform.eulerAngles.x - 270f);
        }
        else {
            _slider.value = -1f * _controller.CinemachineCameraTarget.transform.eulerAngles.x;
        }
    }
}