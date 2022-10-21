using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardStateMachine : StateMachine
{
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Reference to NavMeshAgent")]
    public NavMeshAgent GuardNavMeshAgent;
    [Tooltip("Reference to Player gameobject")]
    public GameObject PlayerGameObject;
    [Tooltip("Reference to area gameobject; Defines what level / room the enemy resides in")]
    public GameObject AreaGameObject;

    [Tooltip("Point system for how aware guard is of player: awareness <= 100 = low awareness; awareness > 100 = high awareness and pursuit")]
    public float Awareness = 0f;
    [Tooltip("Flag for if guard is in pursuit of player: if pursuiting, guard is in high awareness and is moving / surveying towards the player (or last known position of player")]
    public bool Pursuit = false;

    /*** FIELD OF VIEW ***/
    [Tooltip("Layers that the target (what they will react to) has")]
    public LayerMask TargetMask;
    [Tooltip("Layers that cut off vision")]
    public LayerMask ObstructionMask;
    [Tooltip("Patrol line material")]
    public Material PatrolLineMaterial;

    /*** PATROL POINTS ***/
    [Space(10)]
    [Header("Patrol Info")]
    [Tooltip("Contains a game object whose children are all known control points")]
    public GameObject PatrolPointParentGameObject;
    [Tooltip("Holds list of patrol point gameobjects")]
    [HideInInspector]
    public List<GameObject> PatrolPointList = new List<GameObject>();
    [Tooltip("Which patrol point is the guard working towards")]
    [HideInInspector]
    public GameObject CurrentPatrolPoint;
    [Tooltip("Index of currentPatrolPoint in the list")]
    public int CurrentPatrolPointIndex;
    [Tooltip("Where the guard is moving towards")]
    [HideInInspector]
    public GameObject Target;
    [Tooltip("PatrolPoint marking where the guard last sensed the player")]
    [HideInInspector]
    public GameObject PursuitPatrolPoint;

    [Space(10)]
    [Header("Audio Info")]
	[Tooltip("If the character is grounded or not")]
	public bool Grounded = true;
	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check")]
	public float GroundedRadius = 0.5f;
	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;
	[Tooltip("What the guard object is colliding with")]
	public Collider[] colliderArray;
	[Tooltip("Current ground info")]
	public GroundInfo GroundInfo;

    /*** STATES ***/
    [HideInInspector]
    public Moving MovingState;
    [HideInInspector]
    public Survey SurveyState;
    [HideInInspector]
    public FieldOfView FOV;

    /******* PRIVATE VARIABLES *******/
    // holds reference to the player controller script
    private PlayerController _controller;

    // initial guard data for respawning
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private GameObject _initialPatrolPoint;
    private int _initialPatrolPointIndex;
    private GameObject _initialTarget;

    // focus info
    private Focusable _focusable;
    private LineRenderer _patrolPathLineRenderer;

    private void Awake() {
        // initialize components
        GuardNavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        FOV = GetComponent<FieldOfView>();
        FOV.GuardStateMachine = this;

        _controller = PlayerGameObject.GetComponent<PlayerController>();

        // initialize patrol points
        InitializePatrolPoints();

        // define states
        MovingState = new Moving(this);
        SurveyState = new Survey(this);

        _initialPosition = this.gameObject.transform.position;
        _initialRotation = this.gameObject.transform.rotation;

        _initialPatrolPoint = CurrentPatrolPoint;
        _initialPatrolPointIndex = CurrentPatrolPointIndex;
        _initialTarget = Target;

        // initalize line renderer for drawing patrol path
        _patrolPathLineRenderer = gameObject.AddComponent<LineRenderer>();
        _patrolPathLineRenderer.material = PatrolLineMaterial;
        _patrolPathLineRenderer.startWidth = 0.05f;
        _patrolPathLineRenderer.endWidth = 0.05f;
        _patrolPathLineRenderer.startColor = Color.red;
        _patrolPathLineRenderer.endColor = Color.red;
        
        List<Vector3> pos = new List<Vector3>();
        _patrolPathLineRenderer.positionCount = 0;
        _patrolPathLineRenderer.SetPositions(pos.ToArray());
        _patrolPathLineRenderer.useWorldSpace = true;

        _focusable = this.transform.GetChild(2).gameObject.GetComponent<Focusable>();
    }

    void FixedUpdate() {
        // slowly decay awareness over time
        if (Awareness > 0f && !Pursuit) Awareness = Math.Max(Awareness - (3f * Time.deltaTime), 0f);

        // add awareness value based on guard FOV
        FOVAwareness();

        GroundedCheck();
    }

    protected override State GetInitialState() {
        return MovingState;
    }

    void Update() {
        // if target has become focused, visualize patrol path
        if (!_focusable.Traced && _focusable.Focused) {
            VisualizePatrolPathLine(true);
            _focusable.Traced = true;
        }

        // if target is no longer focused, remove visual patrol path
        if (_focusable.Traced && !_focusable.Focused) {
            VisualizePatrolPathLine(false);
            _focusable.Traced = false;
        }
    }

    // increases awareness based on FOV variables
    private void FOVAwareness() {
        // if can't see player, nothing needs to be done
        if (!FOV.CanSeePlayer || !IsPlayerInSameLevel()) return;

        // calculate distance between guard and player
        float distance = Vector3.Distance(transform.position, PlayerGameObject.transform.position);

        // get the angle between where the guard is facing and where the player is
        Vector3 targetDir = PlayerGameObject.transform.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        // if the player is on the edge of the FOV, start to drop off the amount of awareness
        float FOVEdgeDropOff = 1.0f;
        if (angle > 45f && distance > FOV.Radius * (2 / 3)) {
            FOVEdgeDropOff = angle - 44f;
        }

        // awareness += (difference between the FOV radius and distance between guard and player) / FOV edge drop off value (if the player is on the edge then it drops off)
        Awareness += 5f * ((FOV.Radius - distance) / FOVEdgeDropOff) * Time.deltaTime;

        // check if guard should pursuit player
        CheckPursuit();
    }

    // fills list of patrolPoints and sets the first points
    private void InitializePatrolPoints() {
        // fill _patrolPointList
        Transform[] patrolPointArray = PatrolPointParentGameObject.GetComponentsInChildren<Transform>();
        foreach (Transform patrolPointTransform in patrolPointArray)
        {
            if (patrolPointTransform.gameObject != PatrolPointParentGameObject) PatrolPointList.Add(patrolPointTransform.gameObject);
        }

        // set current patrol point to the first one in the list
        CurrentPatrolPoint = PatrolPointList[0];
        CurrentPatrolPointIndex = 0;

        Target = CurrentPatrolPoint;
    }

    // enables pursuit if awareness is high and not currently pursuiting
    public void CheckPursuit() {
        if (Awareness <= 100f) return;
        if (Awareness >= 200f) {
            string lossReason = "You were caught by a guard";
			string hint = "Watch for the awareness indicator above a guards head. If it's yellow, they will be investigating your location";
			_controller.OnMissionFail(lossReason, hint);
            return;
        }

        // create patrol point at current player position
        GameObject newPatrolPoint = new GameObject();
        newPatrolPoint.transform.position = new Vector3(PlayerGameObject.transform.position.x, PlayerGameObject.transform.position.y + 1f, PlayerGameObject.transform.position.z);

        // add and modify patrolPoint variables
        newPatrolPoint.AddComponent<PatrolPoint>();
        newPatrolPoint.GetComponent<PatrolPoint>().Survey = true;
        newPatrolPoint.GetComponent<PatrolPoint>().SurveySpeed = 100f;
        newPatrolPoint.GetComponent<PatrolPoint>().SurveyIterations = UnityEngine.Random.Range(1,3);
        newPatrolPoint.GetComponent<PatrolPoint>().MinSurveyAngle = 0f;
        newPatrolPoint.GetComponent<PatrolPoint>().MaxSurveyAngle = 359f;

        // destroy old PursuitPatrolPoint
        if (PursuitPatrolPoint != null) {
            Destroy(PursuitPatrolPoint);
        }

        // set new patrol point as current
        CurrentPatrolPoint = newPatrolPoint;
        Target = newPatrolPoint;
        PursuitPatrolPoint = newPatrolPoint;

        // start moving towards new patrol point
        ChangeState(MovingState);

        // pursuit player if not already
        if (!Pursuit) {
            Pursuit = true;

            // decrement the PatrolPointIndex so the guard will go back to the patrol point after completing pursuit
            CurrentPatrolPointIndex = Math.Abs((CurrentPatrolPointIndex - 1) % PatrolPointList.Count);
        }
    }

    // resets any necessary variables when the player respawns
    public void ResetOnPlayerRespawn() {
        Awareness = 0f;
        Pursuit = false;

        // reset guard to initial position
        bool warpSucceed = GuardNavMeshAgent.Warp(_initialPosition);
        if (!warpSucceed) {
            Debug.Log("Warp failed when resetting Guard");
            Debug.Log("Attempted to warp to position: " + _initialPosition);
        }
        
        this.gameObject.transform.rotation = _initialRotation;

        // reset patrol info
        CurrentPatrolPoint = _initialPatrolPoint;
        CurrentPatrolPointIndex = _initialPatrolPointIndex;
        Target = _initialTarget;

        ChangeState(MovingState);
    }

    // checks if player is in the same level by checking the enemy parent
    public bool IsPlayerInSameLevel() {
        return (AreaGameObject == _controller.AreaGameObject);
    }

    // sets up line renderer to draw the patrol path of the guard if focused
    private void VisualizePatrolPathLine(bool on) {

        // add patrol point to line renderer list
        List<Vector3> pos = new List<Vector3>();
        NavMeshPath path = new NavMeshPath();
        Vector3[] corners;

        // if turning visual path off
        if (!on) {
            _patrolPathLineRenderer.positionCount = 0;
            _patrolPathLineRenderer.SetPositions(pos.ToArray());
            _patrolPathLineRenderer.useWorldSpace = true;
            return;
        }

        for (int i = 0; i < PatrolPointList.Count; i++) {
            // get path between current and next patrol point
            NavMesh.CalculatePath(PatrolPointList[i].transform.position, PatrolPointList[((i+1) % PatrolPointList.Count)].transform.position, NavMesh.AllAreas, path);
            corners = path.corners;

            // add the path to the line renderer
            foreach (Vector3 corner in corners) {
                pos.Add(corner);
            }
        }

        // set final variables
        _patrolPathLineRenderer.positionCount = pos.Count;
        _patrolPathLineRenderer.SetPositions(pos.ToArray());
        _patrolPathLineRenderer.useWorldSpace = true;
    }

    private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		
		// set ground info based on current collider info
		colliderArray = Physics.OverlapSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		Grounded = (colliderArray.Length > 0) ? true : false;
		SetGroundInfo(colliderArray);
	}

    // sets the groundInfo script based on latest collider info
	private void SetGroundInfo(Collider[] colliderArray) {
		foreach (Collider collider in colliderArray) {
			if (collider.gameObject.layer == 6) {
				GroundInfo currentGroundInfo = collider.gameObject.GetComponent<GroundInfo>();
				GroundInfo = currentGroundInfo;
				return;
			}
		}
	}

    // void OnDrawGizmos() {
    //     for (int i = 0; i < path.corners.Length - 1; i++)
    //         {
    //             Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
    //         }
    // }
}