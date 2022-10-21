using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FocusInstructionTextScript : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object to access player info")]
    public GameObject PlayerObject;

    [Tooltip("Holds reference to the door blocker object")]
    public GameObject DoorBlockerObject;
    [Tooltip("Holds reference to the guard in room 4")]
    public GameObject GuardObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject NextRoomInstructionObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private PlayerInput _input;
    private Focusable _guardFocusableScript;
    private GameObject _objectiveButton;
    private GameObject _completeButton;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
        _input = PlayerObject.GetComponent<PlayerInput>();
        _guardFocusableScript = GuardObject.transform.GetChild(2).gameObject.GetComponent<Focusable>();

        //Change text based on input device
        // if (_input.currentControlScheme == "Gamepad") {
        //     gameObject.GetComponent<TextMeshPro>().SetText("Hold Left Shoulder button while moving to sprint");
        // }

        // fetch objective buttons
        _objectiveButton = gameObject.transform.GetChild(0).gameObject;
        _completeButton = gameObject.transform.GetChild(1).gameObject;
    }

    private void Update() {
        if (_guardFocusableScript.Focused) {
            DoorBlockerObject.SetActive(false);

            // update UI
            gameObject.GetComponent<TMP_Text>().fontStyle = gameObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            gameObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            _objectiveButton.SetActive(false);
            _completeButton.SetActive(true);

            NextRoomInstructionObject.SetActive(true);
        }
    }
}