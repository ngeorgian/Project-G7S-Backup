using UnityEngine;
using TMPro;

public class RespawnButton : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object")]
    public PlayerController PlayerController;
    [Tooltip("Holds reference to the loss panel")]
    public GameObject LossPanelGameObject;

    [Space(10)]
	[Header("Room Info")]
    [Tooltip("Holds reference to the current room's UI game object")]
    public GameObject CurrentRoomUIGameObject;
    [Tooltip("Holds reference to the current room's entrance transition game object")]
    public GameObject EntranceTransitionGameObject;
    [Tooltip("Holds reference to the next room's door blocker game object")]
    public GameObject DoorBlockerGameObject;

    // function for when player loses and clicks the button to load last checkpoint
    public void OnRespawnButtonClick() {
        // reactivate the entrance transition
        if (EntranceTransitionGameObject != null) EntranceTransitionGameObject.SetActive(true);
        if (DoorBlockerGameObject != null) DoorBlockerGameObject.SetActive(true);

        // reset objective UI
        if (CurrentRoomUIGameObject.transform.GetChild(0).gameObject.activeSelf) CurrentRoomUIGameObject.transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i < CurrentRoomUIGameObject.transform.childCount; i ++) {
            if (i != 0) CurrentRoomUIGameObject.transform.GetChild(i).gameObject.SetActive(false);
            if ((CurrentRoomUIGameObject.transform.GetChild(i).GetComponent<TMP_Text>().fontStyle & FontStyles.Strikethrough) != 0) CurrentRoomUIGameObject.transform.GetChild(i).GetComponent<TMP_Text>().fontStyle ^= FontStyles.Strikethrough;
            CurrentRoomUIGameObject.transform.GetChild(i).GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 255);
            CurrentRoomUIGameObject.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
            CurrentRoomUIGameObject.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
        }

        // respawn the player
        PlayerController.Respawn();
    }
}