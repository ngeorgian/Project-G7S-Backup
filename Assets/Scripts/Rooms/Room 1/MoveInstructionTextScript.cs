using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MoveInstructionTextScript : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object to access player info")]
    public GameObject PlayerObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject SprintInstructionObject;
    //public InputAction moveInputAction;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private PlayerInput _input;
    private GameObject _objectiveButton;
    private GameObject _completeButton;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
        _input = PlayerObject.GetComponent<PlayerInput>();

        // change text based on input device
        if (_input.currentControlScheme == "Gamepad") {
            gameObject.GetComponent<TextMeshPro>().SetText("Use Left Stick to move");
        }

        // fetch objective buttons
        _objectiveButton = gameObject.transform.GetChild(0).gameObject;
        _completeButton = gameObject.transform.GetChild(1).gameObject;
    }

    private void Update() {
        // once player moves, display sprint instruction and mark move instruction as complete
        if (_playerController.speed >= 1.5f) {
            SprintInstructionObject.SetActive(true);

            // update UI
            gameObject.GetComponent<TMP_Text>().fontStyle = gameObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            gameObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            _objectiveButton.SetActive(false);
            _completeButton.SetActive(true);
        }
    }
}