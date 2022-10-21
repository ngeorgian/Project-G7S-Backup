using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using Cinemachine;
using TMPro;

/* Note: animations are called via the controller for both the character and capsule using animator null checks*/
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
public class PlayerController : MonoBehaviour
{
	/******* PUBLIC VARIABLES *******/
	[Header("Player Movement")]
	[Tooltip("Current speed of the character in m/s")]
	public float speed;
	[Tooltip("Normal move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Tiptoe speed of the character in m/s")]
	public float TiptoeSpeed = 2.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;

	[Space(10)]
	[Header("Focus Ability")]
	[Tooltip("How long a player focused on a particular object ")]
	public float FocusTime = 0f;
	[Tooltip("Object of what player is currently focusing on")]
	public GameObject CurrentFocusedObject;
	[Tooltip("Last focused object")]
	public GameObject LastFocusedObject;
	[Tooltip("World timescale")]
	public float Timescale = 1f;
	[Tooltip("Focus line material")]
    public Material FocusLineMaterial;

	[Space(10)]
	[Header("Player Spawn")]
	[Tooltip("Position of player spawn location")]
	public Vector3 PlayerSpawnPosition = new Vector3(0f, 0f, 0f);
	[Tooltip("Rotation of player spawn location")]
	public Vector3 PlayerSpawnRotation = new Vector3(0f, 90f, 0f);
	[Tooltip("References the main canvas object")]
	public GameObject MainCanvasObject;
	[Tooltip("References the loss panel game object")]
	public GameObject LossPanelGameObject;
	[Tooltip("Flag to detemrine if player is in loss screen")]
	public bool PlayerLoss = false;
	[Tooltip("Message that says why the player lost")]
	public string PlayerLossMessage = "";
	[Tooltip("Message that gives a hint to the player in the loss screen")]
	public string PlayerHintMessage = "";
	[Tooltip("References the parent gameObject whose children are all of the enemies of the current level")]
	public GameObject EnemyParentGameObject;
	[Tooltip("Reference to area gameobject; Defines what level / room the enemy resides in")]
    public GameObject AreaGameObject;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true;
	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.5f;
	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;
	[Tooltip("What the player object is colliding with")]
	public Collider[] colliderArray;
	[Tooltip("Current ground info")]
	public GroundInfo groundInfo;

	[Header("Cinemachine")]
	[Tooltip("Reference to the main camera")]
	public GameObject MainCamera;
	[Tooltip("Camera that is tied to the main camera that handles the virtual camera for the player controller")]
	public GameObject PlayerFollowCamera;
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 89.9f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -89.9f;

	[Space(10)]
	[Header("Audio Mode Info")]
	[Tooltip("Flag for if audio mode is on or off")]
	public bool AudioModeOn = false;
	[Tooltip("Value for how much audio is multiplied by (if audio is off == 0; if audio is on == 1)")]
	public float AudioPercentage = 0f;
	[Tooltip("Determines how long sense change cooldown is")]
	public float SenseChangeCooldownTime = 5f;
	[Tooltip("Determines percentage volume is heard")]
	public float VolumeRatio = 0f;

	[Space(10)]
	[Header("UI Info")]
	[Tooltip("Holds reference to the next room's entrance transition game object")]
	public GameObject CompassObject;

	/******* PRIVATE VARIABLES *******/
	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;
	private float _senseChangeDelta;
	
	// focus
	private string _focusedObjectType;
	private float _defaultPlayerFollowCameraFOV;

	private CharacterController _controller;
	private PlayerAssetsInputs _input;
	private PlayerAudio _audio;
	private const float _threshold = 0.01f;

	private void Awake()
	{
		// get a reference to our main camera
		if (MainCamera == null)
		{
			MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_controller = GetComponent<CharacterController>();
		_input = GetComponent<PlayerAssetsInputs>();
		_audio = GetComponent<PlayerAudio>();

		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
		_senseChangeDelta = 0f;

		_defaultPlayerFollowCameraFOV = PlayerFollowCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
	}

	private void FixedUpdate()
	{
		// if loss screen is up, disable player controls
		if (PlayerLoss) return;

		// update player movement info
		JumpAndGravity();
		GroundedCheck();
		Move();
		Focus();
		SenseChange();
	}

	private void LateUpdate()
	{
		CameraRotation();
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

	private void CameraRotation()
	{
		// Don't allow for player to move when focusing in audiomode
		if (_input.focus && AudioModeOn) {
			return;
		}

		// if there is an input
		if (_input.look.sqrMagnitude >= _threshold)
		{
			_cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
			_rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Update Cinemachine camera target pitch
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

			// rotate the player left and right
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{
		// Don't allow for player to move when focusing in audiomode
		if (_input.focus && AudioModeOn) {
			return;
		}

		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed;
		if (_input.tiptoe) {
			targetSpeed = TiptoeSpeed;
		} else {
			targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
		}

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			speed = Mathf.Round(speed * 1000f) / 1000f;
		}
		else
		{
			speed = targetSpeed;
		}

		// normalise input direction
		Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (_input.move != Vector2.zero)
		{
			// move
			inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
		}

		// move the player
		_controller.Move(inputDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

	private void JumpAndGravity()
	{
		if (Grounded)
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;

			// stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
			{
				_verticalVelocity = -2f;
			}

			// Jump
			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = JumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}

			// if we are not grounded, do not jump
			_input.jump = false;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	private void Focus() {
		if (_input.focus) {
			// if focus is activated in VISUAL mode
			if (!AudioModeOn) {
				Debug.Log("not audio mode");
				int layerMask = LayerMask.GetMask("Ground", "Obstruction");
				RaycastHit hit;

				// if focus hits an unfocused object
				if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, 50f, layerMask) 
				&& hit.transform.gameObject.GetComponent<Focusable>() != null && !hit.transform.gameObject.GetComponent<Focusable>().Focused && hit.transform.gameObject.GetComponent<Focusable>().VisualFocusable) 
				{
					// Debug.Log("Focusing on: " + hit.transform.gameObject.name);

					// if hit object is a new focusable object
					if (CurrentFocusedObject != hit.transform.gameObject) {
						FocusTime = Time.fixedDeltaTime;
						CurrentFocusedObject = hit.transform.gameObject;
					} else {
						// if focusThreshold reached, set new target as focused
						if (FocusTime >= CurrentFocusedObject.GetComponent<Focusable>().FocusThreshold) {
							if (LastFocusedObject != null) LastFocusedObject.GetComponent<Focusable>().Focused = false;

							CurrentFocusedObject.GetComponent<Focusable>().Focused = true;
							LastFocusedObject = CurrentFocusedObject;
							FocusTime = 0f;
							return;
						}

						FocusTime += Time.fixedDeltaTime;
					}
				}
				// if focus object is not found 
				else 
				{
					Debug.Log("object not found");
					// Debug.DrawRay(transform.position, transform.TransformDirection (Vector3.forward) *1000, Color.white);
					// Debug.Log("Did not Hit");

					FocusTime = Math.Max(FocusTime - Time.fixedDeltaTime, 0f);
					CurrentFocusedObject = null;
				}
			}
			// if focus is activated in AUDIO mode
			else {
				FocusTime += Time.fixedDeltaTime;

				// lower world timescale to things down while player focuses
				if (Timescale > 0.1f) Timescale = Math.Max(Timescale - (Time.fixedDeltaTime), 0.1f);

				// unlock the cursor for the player if not unlocked
				if (Cursor.lockState == CursorLockMode.Locked) {
					Cursor.lockState = CursorLockMode.None;
					Cursor.lockState = CursorLockMode.Confined;
					Cursor.visible = true;
				}
			}
		} else {
			FocusTime = 0f;
			CurrentFocusedObject = null;

			// lock the cursor for the player if unlocked
			if (Cursor.lockState != CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// set visual camera zoom based on focus time
		PlayerFollowCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = _defaultPlayerFollowCameraFOV - (FocusTime * 3);

		// reset timescale if necessary
		if (FocusTime <= 0 && Timescale < 1f) Timescale = Math.Min(Timescale + (Time.fixedDeltaTime), 1f);
		Time.timeScale = Timescale;
	}

	private void SenseChange() {
		// update senseChange cooldown delta
		if (_senseChangeDelta > 0f) _senseChangeDelta -= Time.fixedDeltaTime;

		// update volumeRatio
		if (AudioModeOn && VolumeRatio < 1f) VolumeRatio = Mathf.Min(VolumeRatio + (Time.fixedDeltaTime / SenseChangeCooldownTime), 1f);
		if (!AudioModeOn && VolumeRatio > 0f) VolumeRatio = Mathf.Max(VolumeRatio - (Time.fixedDeltaTime / SenseChangeCooldownTime), 0f);

		if (_input.senseChange && _senseChangeDelta <= 0f) {
			// set cooldown
			_senseChangeDelta = SenseChangeCooldownTime;
			AudioModeOn = !AudioModeOn;
			_input.senseChange = false;
		}

		return;
	}

	// sets the groundInfo script based on latest collider info
	private void SetGroundInfo(Collider[] colliderArray) {
		foreach (Collider collider in colliderArray) {
			if (collider.gameObject.layer == 6) {
				GroundInfo currentGroundInfo = collider.gameObject.GetComponent<GroundInfo>();
				groundInfo = currentGroundInfo;
				return;
			}
		}
	}

	// resets player and moves them to last checkpoint
	public void Respawn() {
		// move the player to the respawn points
		_controller.enabled = false;
		transform.position = PlayerSpawnPosition;
		transform.eulerAngles = PlayerSpawnRotation;
		_controller.enabled = true;

		// change player camera to face spawn direction
		_cinemachineTargetPitch = 0.0f;
		CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		// lock the cursor for the player
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		
		// reset player variables
		_audio.playerLossByLoudFootstep = false;
		VolumeRatio = 0f;
		AudioModeOn = false;
		for (int i = 0; i < _audio.AudioVisualParentObject.transform.childCount; i++) {
			Destroy(_audio.AudioVisualParentObject.transform.GetChild(i).gameObject);
		}

		CurrentFocusedObject = null;
		LastFocusedObject = null;
		FocusTime = 0f;

		PlayerLoss = false;

		// reset the UI
		ToggleLossUI(false);

		// reset enemies in current level
		if (EnemyParentGameObject != null) ResetEnemiesOnRespawn();
	}

	// turns on and off the loss panel ui and sets all other ui opposite
	public void ToggleLossUI(bool on) {
		for (int i = 0; i < MainCanvasObject.transform.childCount; i++) {
			if (MainCanvasObject.transform.GetChild(i).gameObject.name != MainCanvasObject.name) {
				if (MainCanvasObject.transform.GetChild(i).gameObject.name == LossPanelGameObject.name) LossPanelGameObject.SetActive(on);
				// else if (MainCanvasObject.transform.GetChild(i).gameObject.name == _audio.AudioModePanelGameObject.name) _audio.AudioModePanelGameObject.SetActive(false); // always keep audio panel off when toggling loss UI
				else MainCanvasObject.transform.GetChild(i).gameObject.SetActive(!on);
			}
		}
	}

	// places player in lose screen with info about why they lost
	public void OnMissionFail(string lossReason, string hint) {
		PlayerLoss = true;

		// unlock the cursor for the player
		Cursor.lockState = CursorLockMode.None;
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;

		// set loss panel texts and display it
		LossPanelGameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().SetText(lossReason);
		LossPanelGameObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().SetText(hint);
		ToggleLossUI(true);
	}

	// resets enemies to their initial positions when player respawns
	public void ResetEnemiesOnRespawn() {
		Transform[] EnemyTransformList = EnemyParentGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform EnemyTransform in EnemyTransformList) {
			switch (EnemyTransform.parent.name) {
				// enemy type == guard
				case "Guards":
					EnemyTransform.gameObject.GetComponent<GuardStateMachine>().ResetOnPlayerRespawn();
					EnemyTransform.GetChild(2).gameObject.GetComponent<Focusable>().Focused = false;
					break;
				default:
					// Debug.Log("enemy type " + EnemyTransform.gameObject.name + " not supported");
					break;
			}
		}
	}

	private string GetFocusedTargetType(GameObject newFocusedObject) {
		if (newFocusedObject.GetComponent<GuardStateMachine>() != null) return "Guard";
		else {
			return "Undefined";
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (Grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
	}
}