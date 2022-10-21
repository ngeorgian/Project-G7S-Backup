using UnityEngine;

public class RoomFourEntranceTransition : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object")]
    public GameObject PlayerObject;
    [Tooltip("Holds Reference to the next objective object for the compass arrow to point to")]
    public GameObject CompassTargetObject;
    [Tooltip("Holds reference to the focus instruction text object")]
    public GameObject FocusInstructionText;
    [Tooltip("References the parent gameObject whose children are all of the enemies of the current level")]
	public GameObject EnemyParentGameObject;
    [Tooltip("Reference to area gameobject; Defines what level / room the enemy resides in")]
    public GameObject AreaGameObject;
    [Tooltip("Holds reference to the previous room's UI gameObject")]
    public GameObject RoomThreeUIObject;
    [Tooltip("Holds reference to the previous room's door blocker gameObject")]
    public GameObject RoomThreeDoorBlocker;
    [Tooltip("Holds reference to the previous room's enemy parent game object")]
    public GameObject RoomThreeEnemyParentGameObject;

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

            // activate room 4 UI
            for (int i = 0; i < RoomThreeUIObject.transform.childCount; i++) {
                RoomThreeUIObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            FocusInstructionText.SetActive(true);

            // update compass
            _playerController.CompassObject.GetComponent<MainCompass>().Target = CompassTargetObject;

            // deactivate trigger object
            gameObject.SetActive(false);

            // block previous room and deactivate enemies
            RoomThreeDoorBlocker.SetActive(true);
            RoomThreeEnemyParentGameObject.SetActive(false);
        }
    }
}