using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TiptoeInstructionTextScript : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object to access player info")]
    public GameObject playerObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject QuietWalkInstructionObject;
    //public InputAction moveInputAction;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private PlayerAssetsInputs _input;
    private PlayerInput _rawInput;
    private GameObject _objectiveButton;
    private GameObject _completeButton;

    private void Awake() {
        _playerController = playerObject.GetComponent<PlayerController>();
        _input = playerObject.GetComponent<PlayerAssetsInputs>();
        _rawInput = playerObject.GetComponent<PlayerInput>();

        //Change text based on input device
        if (_rawInput.currentControlScheme == "Gamepad") {
            gameObject.GetComponent<TextMeshProUGUI>().SetText("Use Left shoulder while moving to move quietly");
        }

        // fetch objective buttons
        _objectiveButton = gameObject.transform.GetChild(0).gameObject;
        _completeButton = gameObject.transform.GetChild(1).gameObject;
    }

    private void Update() {
        // Once player moves, display sprint instruction and disable move instructions
        if (_input.tiptoe && _playerController.speed > 0.5f && _playerController.speed <= 2f) {
            // update UI
            gameObject.GetComponent<TMP_Text>().fontStyle = gameObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            gameObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            _objectiveButton.SetActive(false);
            _completeButton.SetActive(true);

            // display next objective
            QuietWalkInstructionObject.SetActive(true);
        }
    }
}