using UnityEngine;
using TMPro;

public class RoomFiveExitTransition : MonoBehaviour {
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

    [Space(10)]
	[Header("UI Info")]
    [Tooltip("Holds Reference to the next objective object for the compass arrow to point to")]
    public GameObject CompassTargetObject;
    [Tooltip("Text from room 4 to turn off")]
    public GameObject SneakInstructionText;
    [Tooltip("Holds reference to the next room's UI game object")]
    public GameObject NewRoomUIGameObject;
    [Tooltip("Holds reference to the respawn button game object")]
    public GameObject RespawnButton;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // when player collides with object, set instructions object to true
        if (other.gameObject.tag == "Player") {

            // set player spawn info
            _playerController.PlayerSpawnPosition = PlayerSpawnPosition;
            _playerController.PlayerSpawnRotation = PlayerRotationPosition;

            // update UI
            SneakInstructionText.GetComponent<TMP_Text>().fontStyle = SneakInstructionText.GetComponent<TMP_Text>().fontStyle | FontStyles.Strikethrough;
            SneakInstructionText.GetComponent<TMP_Text>().color = new Color32(100, 255, 100, 215);
            SneakInstructionText.transform.GetChild(0).gameObject.SetActive(false);
            SneakInstructionText.transform.GetChild(1).gameObject.SetActive(true);

            // update respawn info
            RespawnButton.GetComponent<RespawnButton>().CurrentRoomUIGameObject = NewRoomUIGameObject;
            RespawnButton.GetComponent<RespawnButton>().EntranceTransitionGameObject = EntranceTransitionGameObject;
            RespawnButton.GetComponent<RespawnButton>().DoorBlockerGameObject = null;

            // deactivate trigger object
            gameObject.SetActive(false);
        }
    }
}