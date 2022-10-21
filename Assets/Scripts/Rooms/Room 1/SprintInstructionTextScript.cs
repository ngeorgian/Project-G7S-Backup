using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SprintInstructionTextScript : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object to access player info")]
    public GameObject PlayerObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject NextAreaInstructionObject;

    [Tooltip("Holds reference to the door blocker object")]
    public GameObject DoorBlockerObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private PlayerInput _input;
    private GameObject _objectiveButton;
    private GameObject _completeButton;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
        _input = PlayerObject.GetComponent<PlayerInput>();

        //Change text based on input device
        if (_input.currentControlScheme == "Gamepad") {
            gameObject.GetComponent<TextMeshPro>().SetText("Hold Left Shoulder button while moving to sprint");
        }

        // fetch objective buttons
        _objectiveButton = gameObject.transform.GetChild(0).gameObject;
        _completeButton = gameObject.transform.GetChild(1).gameObject;
    }

    private void Update() {
        if (_playerController.speed >= 5.5f) {
            DoorBlockerObject.SetActive(false);
            NextAreaInstructionObject.SetActive(true);

            // update UI
            gameObject.GetComponent<TMP_Text>().fontStyle = gameObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            gameObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            _objectiveButton.SetActive(false);
            _completeButton.SetActive(true);
        }
    }
}