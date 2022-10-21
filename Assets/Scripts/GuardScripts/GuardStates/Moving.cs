using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : State {
    private GuardStateMachine _stateMachine;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private GameObject _playerObject;

    public Moving(GuardStateMachine stateMachine) : base("Moving", stateMachine) {
    _stateMachine = stateMachine;
    _navMeshAgent = _stateMachine.GuardNavMeshAgent;
    _playerObject = _stateMachine.PlayerGameObject;
    }

    public override void UpdatePhysics() {
        // move the agent towards the current target
        _navMeshAgent.destination = _stateMachine.Target.transform.position;

        // check if we've reached the target, move to next target or survey
        if (!_navMeshAgent.pathPending && (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance) && (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)) {
            if (_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().Survey) _stateMachine.ChangeState(_stateMachine.SurveyState);
            else {
                // set target to next patrol point
                _stateMachine.CurrentPatrolPointIndex = (_stateMachine.CurrentPatrolPointIndex + 1) % _stateMachine.PatrolPointList.Count;
                _stateMachine.CurrentPatrolPoint = _stateMachine.PatrolPointList[_stateMachine.CurrentPatrolPointIndex];
                _stateMachine.Target = _stateMachine.CurrentPatrolPoint;
            }
        }
    }

    public override void Enter() {
        _navMeshAgent.destination = _stateMachine.Target.transform.position;
    }

    public override void Exit() {
        _navMeshAgent.ResetPath();
    }
}