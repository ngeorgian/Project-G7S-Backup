using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowAwarenessBar : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    public GameObject GuardGameObject;
    public GameObject HighAwarenessButton;

    /******* PRIVATE VARIABLES *******/
    private GuardStateMachine _stateMachine;
    private Image _awarenessImage;

    void Awake() {
        _stateMachine = GuardGameObject.GetComponent<GuardStateMachine>();
        _awarenessImage = GetComponent<Image>();
    }

    void Update()
    {
        // fill the awareness bar as needed
        _awarenessImage.fillAmount = Math.Min(_stateMachine.Awareness / 100f, 1f);
        transform.LookAt(_stateMachine.PlayerGameObject.transform);

        // if awareness is over 100, change to high awareness
        if (_stateMachine.Awareness > 100f) {
            HighAwarenessButton.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
