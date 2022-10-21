using UnityEngine;

public class RoomTwoEntranceTransition : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object")]
    public GameObject PlayerObject;
    [Tooltip("Holds Reference to the next objective object for the compass arrow to point to")]
    public GameObject CompassTargetObject;
    [Tooltip("Holds reference to the tiptoe instruction text object")]
    public GameObject TiptoeInstructionObject;
    [Tooltip("Holds reference to the previous room's UI gameObject")]
    public GameObject RoomOneUIObject;
    [Tooltip("Holds reference to the previous room's door blocker gameObject")]
    public GameObject RoomOneDoorBlocker;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private PlayerAudio _playerAudio;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
        _playerAudio = PlayerObject.GetComponent<PlayerAudio>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // when player collides with object, set instructions object to true
        if (other.gameObject.tag == "Player") {
            // set audio threshold for room 2
            _playerAudio.playerLossByLoudFootstep = true;
            _playerAudio.playerLossByAudioThreshold = 10f;

            // update compass
            _playerController.CompassObject.GetComponent<MainCompass>().Target = CompassTargetObject;

            // activate room 2 UI
            for (int i = 0; i < RoomOneUIObject.transform.childCount; i++) {
                RoomOneUIObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            TiptoeInstructionObject.SetActive(true);
            gameObject.SetActive(false);

            RoomOneDoorBlocker.SetActive(true);
        }
    }
}