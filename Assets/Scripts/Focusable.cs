using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focusable : MonoBehaviour
{
    [Header("Focus Info")]
    [Tooltip("Total time player needs to focus on the object")]
    public float FocusThreshold;
    [HideInInspector]
    [Tooltip("Flag for if the object is being focused")]
    public bool Focused;
    [Tooltip("Flag for if the object can be focused on in visual mode")]
    public bool VisualFocusable = true;

    [Space(10)]
    [Header("Informational")]
    [Tooltip("Flag that determines if focused object gives player information")]
    public bool Informational;
    [Tooltip("Message to give player information about the object")]
    public string FocusMessage;

    [Space(10)]
    [Header("Traceable")]
    [Tooltip("Flag that determines if enemy is being traced")]
    public bool Traced;
}