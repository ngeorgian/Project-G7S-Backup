using UnityEngine;

public class RoomFiveEntranceTransition : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object")]
    public GameObject PlayerObject;
    [Tooltip("Holds Reference to the next objective object for the compass arrow to point to")]
    public GameObject CompassTargetObject;
    [Tooltip("Holds reference to the sneak instruction text object")]
    public GameObject SneakInstructionText;
    [Tooltip("References the parent gameObject whose children are all of the enemies of the current level")]
	public GameObject EnemyParentGameObject;
    [Tooltip("Reference to area gameobject; Defines what level / room the enemy resides in")]
    public GameObject AreaGameObject;
    [Tooltip("Holds reference to the previous room's UI gameObject")]
    public GameObject RoomFourUIObject;
    [Tooltip("Holds reference to the previous room's door blocker gameObject")]
    public GameObject RoomFourDoorBlocker;
    [Tooltip("Holds reference to the previous room's enemy parent game object")]
    public GameObject RoomFourEnemyParentGameObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;

    private void Awake() {
        _playerController = PlayerObject.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") {
            // set player variables
            other.gameObject.GetComponent<PlayerController>().EnemyParentGameObject = this.EnemyParentGameObject;
            other.gameObject.GetComponent<PlayerController>().AreaGameObject = this.AreaGameObject;

            // update compass
            _playerController.CompassObject.GetComponent<MainCompass>().Target = CompassTargetObject;

            // activate new room UI
            for (int i = 0; i < RoomFourUIObject.transform.childCount; i++) {
                RoomFourUIObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            SneakInstructionText.SetActive(true);

            // deactivate trigger object
            gameObject.SetActive(false);

            // block previous room and deactivate enemies
            RoomFourDoorBlocker.SetActive(true);
            RoomFourEnemyParentGameObject.SetActive(false);
        }
    }
}