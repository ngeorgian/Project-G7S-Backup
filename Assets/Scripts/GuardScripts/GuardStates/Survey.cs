using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survey : State {
    private GuardStateMachine _stateMachine;

    /*** SURVEY ***/
    // how many times left to survey
    private int _surveyIterations = 0;
    // how much time left to survey
    private float _surveyTime = 0f;
    // the lower value angle that the guard surveys to
    private float _minSurveyAngle;
    // the higher value angle that the guard surveys to
    private float _maxSurveyAngle;
    // the angle that the guard is rotating to
    private float _targetAngle;
    // determines if the guard is rotating clockwise (up) or counter-clockwise (down)
    private Vector3 _rotationDirection;

    public Survey(GuardStateMachine stateMachine) : base("Surveying", stateMachine){
        _stateMachine = stateMachine;
    }

    public override void UpdatePhysics() {
        // if survey is done, move to next target
        if (_surveyTime <= 0f && _surveyIterations <= 0) {
            _stateMachine.CurrentPatrolPointIndex = (_stateMachine.CurrentPatrolPointIndex + 1) % _stateMachine.PatrolPointList.Count;
            _stateMachine.CurrentPatrolPoint = _stateMachine.PatrolPointList[_stateMachine.CurrentPatrolPointIndex];
            _stateMachine.Target = _stateMachine.CurrentPatrolPoint;

            // if pursuiting when surveying is done, guard gives up pursuit and goes back to normal patrol
            if (_stateMachine.Pursuit) _stateMachine.Pursuit = false;
            
            _stateMachine.ChangeState(_stateMachine.MovingState);
        }

        // update survey time
        if (_surveyTime >= 0f) _surveyTime -= Time.deltaTime;
        _stateMachine.gameObject.transform.Rotate(_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().SurveySpeed * _rotationDirection * Time.deltaTime);

        // if guard reaches target angle
        if (Math.Abs(_targetAngle - guardRotationValue(_stateMachine.gameObject.transform.localEulerAngles.y)) < 5f) {
            // update rotation direction
            if (_rotationDirection == Vector3.up) _rotationDirection = Vector3.down;
            else _rotationDirection = Vector3.up;

            // update iteration count
            if (_targetAngle == _maxSurveyAngle && _surveyIterations > 0) {
                if (_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().IterateOnMax) _surveyIterations--;
                _targetAngle = _minSurveyAngle;
            } else {
                if (!_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().IterateOnMax) _surveyIterations--;
                _targetAngle = _maxSurveyAngle;
            }
        }
    }

    public override void Enter() {
        _surveyIterations = _stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().SurveyIterations;
        _surveyTime = _stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().SurveyTime;
        _minSurveyAngle = _stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().MinSurveyAngle;
        _maxSurveyAngle = _stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().MaxSurveyAngle;
        _targetAngle = _minSurveyAngle;

        if (_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().StartRotatingClockwise) _rotationDirection = Vector3.up;
        else _rotationDirection = Vector3.down;

        if (_stateMachine.CurrentPatrolPoint.GetComponent<PatrolPoint>().IterateOnMax) _targetAngle = _minSurveyAngle;
        else _targetAngle = _maxSurveyAngle;
    }

    // recalculates rotation value to be within 0 and 360
    private float guardRotationValue(float y) {
        while (y > 360f) y -= 360f;
        while (y < 0f) y += 360f;

        return y;
    }
}