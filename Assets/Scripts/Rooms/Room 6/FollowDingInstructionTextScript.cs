using UnityEngine;
using TMPro;


public class FollowDingInstructionTextScript : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object to access player info")]
    public GameObject PlayerObject;
    [Tooltip("Holds reference to the last sound emitter in room 6")]
    public GameObject FinalSoundEmitterGameObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject VisualModeInstructionTextObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private GameObject _objectiveButton;
    private GameObject _completeButton;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();

        // fetch objective buttons
        _objectiveButton = gameObject.transform.GetChild(0).gameObject;
        _completeButton = gameObject.transform.GetChild(1).gameObject;
    }

    private void Update() {
        if(Vector3.Distance(FinalSoundEmitterGameObject.transform.position, PlayerObject.transform.position) < 5f){
            VisualModeInstructionTextObject.SetActive(true);

            // update UI
            gameObject.GetComponent<TMP_Text>().fontStyle = gameObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            gameObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            _objectiveButton.SetActive(false);
            _completeButton.SetActive(true);
        }
    }
}