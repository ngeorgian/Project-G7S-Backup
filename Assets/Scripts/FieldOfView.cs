using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    /******* PUBLIC VARIABLES *******/
    [Header("FOV Details")]
    [Tooltip("Flag for if player is currently being seen")]
    public bool CanSeePlayer;
    [Tooltip("Total viewing distance in meters")]
    public float Radius;
    [Tooltip("Total viewing angle")]
    [Range(0,360)]
    public float Angle;

    [Space(10)]
    [Header("References")]
    [Tooltip("Player gameobject reference")]
    public GameObject PlayerRef;
    [Tooltip("GuardStateMachine reference")]
    public GuardStateMachine GuardStateMachine;

    private void Start()
    {
        PlayerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, Radius, GuardStateMachine.TargetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, GuardStateMachine.ObstructionMask))
                    CanSeePlayer = true;
                else
                    CanSeePlayer = false;
            }
            else
                CanSeePlayer = false;
        }
        else if (CanSeePlayer)
            CanSeePlayer = false;
    }
}