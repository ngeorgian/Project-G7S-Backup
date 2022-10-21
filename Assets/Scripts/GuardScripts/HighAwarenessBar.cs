using System;
using UnityEngine;
using UnityEngine.UI;

public class HighAwarenessBar : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    public GameObject GuardGameObject;
    public GameObject LowAwarenessButton;

    /******* PRIVATE VARIABLES *******/
    private GuardStateMachine _stateMachine;
    private Image _awarenessImage;

    void Awake() {
        _stateMachine = GuardGameObject.GetComponent<GuardStateMachine>();
        _awarenessImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_stateMachine.Awareness <= 100f) {
            LowAwarenessButton.SetActive(true);
            this.gameObject.SetActive(false);
        }

        _awarenessImage.fillAmount = Math.Min((_stateMachine.Awareness - 100f) / 100f, 1f);
        transform.LookAt(_stateMachine.PlayerGameObject.transform);
    }
}
