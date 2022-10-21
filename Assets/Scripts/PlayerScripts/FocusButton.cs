using UnityEngine;

public class FocusButton : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    public GameObject PlayerGameObject;
    public GameObject TripwireGameObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;

    public void OnFocusButtonClick() {
        Debug.Log("Button hath been clicked for " + TripwireGameObject.gameObject.name);
    }

    void Awake() 
    {

    }

    void Update()
    {

    }
}
