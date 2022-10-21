using System;
using UnityEngine;
using UnityEngine.UI;

public class FocusAwarenessBar : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    public GameObject PlayerGameObject;

    /******* PRIVATE VARIABLES *******/
    private PlayerController _playerController;
    private Image _focusImage;

    private float _focusThreshold = 3f;

    void Awake() {
        _playerController = PlayerGameObject.GetComponent<PlayerController>();
        _focusImage = GetComponent<Image>();
    }

    void Update()
    {
        if (_playerController.CurrentFocusedObject != null) _focusThreshold = _playerController.CurrentFocusedObject.GetComponent<Focusable>().FocusThreshold;
        _focusImage.fillAmount = Math.Min((_playerController.FocusTime / _focusThreshold), 1f);
    }
}
