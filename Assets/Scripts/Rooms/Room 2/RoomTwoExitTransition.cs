using UnityEngine;
using TMPro;

public class RoomTwoExitTransition : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Header("Player Info")]
    [Tooltip("Holds reference to the player object")]
    public GameObject PlayerObject;
    [Tooltip("Updates the player spawn position")]
    public Vector3 PlayerSpawnPosition;
    [Tooltip("Updates the player spawn rotation")]
    public Vector3 PlayerRotationPosition;

    [Space(10)]
	[Header("Transition Info")]
    [Tooltip("Holds reference to the next room's entrance transition game object")]
    public GameObject EntranceTransitionGameObject;
    [Tooltip("Holds reference to the next room's enemy parent game object")]
    public GameObject RoomThreeEnemyParentGameObject;


    [Space(10)]
	[Header("UI Info")]
    [Tooltip("Holds Reference to the next objective object for the compass arrow to point to")]
    public GameObject CompassTargetObject;
    [Tooltip("Holds reference to the next instruction to display to the player")]
    public GameObject QuietWalkInstructionObject;
    [Tooltip("Holds reference to the next room's UI game object")]
    public GameObject NewRoomUIGameObject;
    [Tooltip("Holds reference to the respawn button game object")]
    public GameObject RespawnButton;

    /******* PRIVATE VARIABLES *******/
    private PlayerAudio _playerAudio;
    private PlayerController _playerController;

    private void Awake() {
        _playerAudio = PlayerObject.GetComponent<PlayerAudio>();
        _playerController = PlayerObject.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // When player collides with object, set instructions object to true
        if (other.gameObject.tag == "Player") {
            // Turn off loud footstep loss
            _playerAudio.playerLossByLoudFootstep = false;

            // Set player spawn info
            _playerController.PlayerSpawnPosition = PlayerSpawnPosition;
            _playerController.PlayerSpawnRotation = PlayerRotationPosition;

            // Update UI
            QuietWalkInstructionObject.GetComponent<TMP_Text>().fontStyle = QuietWalkInstructionObject.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            QuietWalkInstructionObject.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            QuietWalkInstructionObject.transform.GetChild(0).gameObject.SetActive(false);
            QuietWalkInstructionObject.transform.GetChild(1).gameObject.SetActive(true);

            // update respawn info
            RespawnButton.GetComponent<RespawnButton>().CurrentRoomUIGameObject = NewRoomUIGameObject;
            RespawnButton.GetComponent<RespawnButton>().EntranceTransitionGameObject = EntranceTransitionGameObject;
            RespawnButton.GetComponent<RespawnButton>().DoorBlockerGameObject = null;

            // update compass
            _playerController.CompassObject.GetComponent<MainCompass>().Target = CompassTargetObject;

            // Deactivate trigger object
            gameObject.SetActive(false);

            // Activate the next room's enemies
            RoomThreeEnemyParentGameObject.SetActive(true);
        }
    }
}