using UnityEditor;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    [Header("Survey Info")]
    [Tooltip("Flag for if surveying will occurr at the patrol point")]
    public bool Survey;
    [Tooltip("Speed that the guard surveys back and forth")]
    public float SurveySpeed;
    [Tooltip("Amount of times guard will look back and forth between angles")]
    public int SurveyIterations;
    [Tooltip("Amount of time guard will survey at this patrol point")]
    public float SurveyTime;
    [Tooltip("Minimum angle guard will look")]
    public float MinSurveyAngle;
    [Tooltip("Amount of time guard will survey at this patrol point")]
    public float MaxSurveyAngle;
    [Tooltip("Determines if guard starts rotating clockwise (low -> high) or counter clockwise (high -> low)")]
    public bool StartRotatingClockwise = true;
    [Tooltip("Determines if guard turns towards the min angle first (true) or the max angle first (false)")]
    public bool IterateOnMax = true;
}