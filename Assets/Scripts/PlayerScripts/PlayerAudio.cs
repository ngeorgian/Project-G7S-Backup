using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class PlayerAudio : MonoBehaviour {
	/******* PUBLIC VARIABLES *******/
	[Header("Tile")]
	[Tooltip("Walking footsteps for tile")]
	public AudioClip[] footstepsTileWalk;
	[Tooltip("Jump land for tile")]
	public AudioClip[] JumpTile;

	[Space(10)]
	[Header("Metal")]
	[Tooltip("Walking footsteps for metal")]
	public AudioClip[] footstepsMetalWalk;
	[Tooltip("Jump land for metal")]
	public AudioClip[] JumpMetal;

	[Space(10)]
	[Header("Loss By Footstep")]
	[Tooltip("If true, player footsteps must be below the audio threshold")]
	public bool playerLossByLoudFootstep = false;
	[Tooltip("Threshold for if player audio must not exceed a certaom amount")]
	public float playerLossByAudioThreshold;

	[Space(10)]
	[Header("Audio Mode Info")]
	[Tooltip("References the GameObject for the audio mode panel which contains the audio UI")]
	public GameObject AudioModePanelGameObject;
	[Tooltip("References the GameObject for the audio visual particle effect prefab")]
	public GameObject AudioVisualParticleSystemObjectPrefab;
	[Tooltip("References the feel text object at the top of the screen")]
	public GameObject FeelTextTopObject;
	[Tooltip("References the feel text object at the bottom of the screen")]
	public GameObject FeelTextBottomObject;
	[Tooltip("References the feel text object at the left of the screen")]
	public GameObject FeelTextLeftObject;
	[Tooltip("References the feel text object at the right of the screen")]
	public GameObject FeelTextRightObject;
	[Tooltip("References the head vertical slider in the audiovisual panel")]
	public GameObject HeadVerticaSliderObject;
	[Tooltip("References the gameobject that is the parent of the audiovisual elements")]
	public GameObject AudioVisualParentObject;
	[Tooltip("References the crosshair gameobject")]
	public GameObject FocusCrosshairObject;

	[Space(10)]
	[Header("Prefab References")]
	[Tooltip("Prefab for the sound sphere emitted for every noise")]
	public GameObject SoundSpherePrefab;
	[Header("Audio Visual Focus Button")]
	public GameObject AudioVisualFocusButtonPrefab;


	/******* PRIVATE VARIABLES *******/
	private float _footstepAudioCooldown;

	// determines if player was grounded on previous frame -> used to determine when to play jump land audio
	private bool _groundedOnPreviousFrame = true;

	// create 2 lists for holding sound spheres ->
	// newSphereList holds sounds sphere references ->
	// at the end of every frame, old spheres are destroyed ->
	// new spheres become old spheres
	private List<GameObject> _newSphereList = new List<GameObject>();
	private List<GameObject> _oldSphereList = new List<GameObject>();

	// player info
	private PlayerController _controller;
	private PlayerAssetsInputs _input;
	
	// UI info
	[SerializeField]
	private Camera _uiCamera;

	// list of all the possible sound sphere types
	private List<string> _soundSphereTypeList = new List<string>(){"EnemySoundSphere", "EnvironmentSoundSphere"};

	private void Start() {
		_controller = this.gameObject.GetComponent<PlayerController>();
		_input = this.gameObject.GetComponent<PlayerAssetsInputs>();
	}

	private void FixedUpdate() {
		GetComponent<AudioSource>().pitch = _controller.Timescale;

		PlayFootsteps();
		PlayJumpLand();
		ClearOldSoundSpheres();
		UpdateFeelText();
		ShowHideAudioPanel();
	}

	// check when player collides with trigger collider
    private void OnTriggerEnter(Collider other) {
        // if the collided object is a sound sphere
        if (_soundSphereTypeList.Contains(other.gameObject.tag) && _controller.VolumeRatio > 0.667f) {
			// grab a reference to the soundsphere script
			SoundSphere soundSphereInfo = other.gameObject.GetComponent<SoundSphere>();

            // calculate distance between enemy and player
            float distance = Vector3.Distance(transform.position, other.gameObject.transform.position);

			// instantiate new visual
			GameObject newAudioVisual = Instantiate(AudioVisualParticleSystemObjectPrefab);
			newAudioVisual.name = other.gameObject.name + " audio visual";
			newAudioVisual.AddComponent<AudioVisual>();

			// modify particle system based on soundsphere info
			ParticleSystem audioVisualParticleSystem = newAudioVisual.GetComponent<ParticleSystem>();
			var mainPS = audioVisualParticleSystem.main;
			mainPS.startSize = other.gameObject.transform.localScale.x * (soundSphereInfo.volume / 2f) * _controller.VolumeRatio;
			mainPS.startColor = soundSphereInfo.audioVisualColor;

			// Set lifetime and speed based on timescale
			newAudioVisual.GetComponent<AudioVisual>().Lifetime = (mainPS.startLifetime.constant / mainPS.simulationSpeed) + 0.25f;
			mainPS.simulationSpeed *= _controller.Timescale;

			// calculate angle between sphere and player forward direction
			Vector3 targetDir = other.gameObject.transform.position - transform.position;
			float signedAngle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);

			// takes the signed angle and maps it to the outside of the screen
			Vector3 perimeterParameters = MapSignedAngleToScreenPerimeter(signedAngle);

			// transform screen positions to world positions
			Vector3 audioVisualNewWorldPosition = _uiCamera.ScreenToWorldPoint(perimeterParameters);
			newAudioVisual.transform.position = audioVisualNewWorldPosition;
			newAudioVisual.transform.localPosition = new Vector3(audioVisualNewWorldPosition.x, audioVisualNewWorldPosition.y, 0f);
			newAudioVisual.transform.SetParent(AudioVisualParentObject.transform);

			// add focus button under sound sphere if audio focus is active
			if (soundSphereInfo.audioFocus) {
				Debug.Log("Audiofocus present");
				GameObject newFocusButton = Instantiate(AudioVisualFocusButtonPrefab);
				newFocusButton.name = newAudioVisual.name + " focus button";

				// modify transform to make button sit in correct spot on screen
				newFocusButton.transform.SetParent(newAudioVisual.gameObject.transform);
				newFocusButton.transform.localPosition = new Vector3(0f, 0f, 0f);
				newFocusButton.transform.localScale = new Vector3(0.165f, 0.165f, 0.165f);

				// Attach button script
				FocusButton focusButtonScript = newFocusButton.GetComponent<FocusButton>();
				focusButtonScript.PlayerGameObject = this.gameObject;
				focusButtonScript.TripwireGameObject = soundSphereInfo.SoundEmitterGameObject;
			}
		}
    }


	private void PlayFootsteps() {
		if (_footstepAudioCooldown > 0f) {
			_footstepAudioCooldown -= Time.deltaTime;
		}

		if (_controller.speed > 0.85f && _footstepAudioCooldown <= 0f) {
			if (!_controller.Grounded || _controller.PlayerLoss) {
				return;
			}

			// determine which audioArray to use based on groundInfo
			AudioClip[] audioArray = FetchFootstepAudioArray();

			// pick random footstep audioClip to play
			int index = UnityEngine.Random.Range (0, audioArray.Length);
			AudioClip footstep = audioArray[index];

			float audioRadius;
			float playedVolume = 0f;
			float volumeRatio = Math.Max(Math.Min((3f *  _controller.VolumeRatio), 3f) - 2f, 0f);

			// play audio and set cooldown
			if (_input.sprint) {
				GetComponent<AudioSource>().PlayOneShot(footstep, _controller.groundInfo.SprintNoise * volumeRatio);
				_footstepAudioCooldown = 0.4f;
				audioRadius = _controller.groundInfo.SprintRadius;
				playedVolume = _controller.groundInfo.SprintNoise * _controller.VolumeRatio;
			} else if (_input.tiptoe && _controller.speed <= 2f){
				GetComponent<AudioSource>().PlayOneShot(footstep, _controller.groundInfo.TiptoeNoise * volumeRatio);
				_footstepAudioCooldown = 0.9f;
				audioRadius = _controller.groundInfo.TiptoeRadius;
				playedVolume = _controller.groundInfo.TiptoeNoise * _controller.VolumeRatio;
			} else {
				GetComponent<AudioSource>().PlayOneShot(footstep, _controller.groundInfo.WalkNoise * volumeRatio);
				_footstepAudioCooldown = 0.6f;
				audioRadius = _controller.groundInfo.WalkRadius;
				playedVolume = _controller.groundInfo.WalkNoise * _controller.VolumeRatio;
			}

			// check if threshold was crossed for loud footsteps
			CheckLossByLoudAudio(audioRadius);

			// create sound sphere
			CreateSoundSphere(audioRadius);

			// create audio visual for the sound
			CreateAudioVisualForPlayerSound(audioRadius);
		}
	}

	// plays jump sound when player lands back on ground after jumping
	private void PlayJumpLand() {
		// if the player landed after jumping
		if (_controller.Grounded && !_groundedOnPreviousFrame) {
			// determine which audioArray to use based on groundInfo
			AudioClip[] audioArray = FetchJumpAudioArray();

			// pick random footstep audioClip to play
			int index = UnityEngine.Random.Range (0, audioArray.Length);
			AudioClip jumpLand = audioArray[index];

			// play the sound
			float volumeRatio = Math.Max(Math.Min((3f *  _controller.VolumeRatio), 3f) - 2f, 0f);
			GetComponent<AudioSource>().PlayOneShot(jumpLand, _controller.groundInfo.JumpNoise * volumeRatio);

			// check if threshold was crossed for loud footsteps
			float audioRadius = _controller.groundInfo.JumpRadius;
			CheckLossByLoudAudio(audioRadius);

			// create sound sphere
			CreateSoundSphere(audioRadius);

			// create audio visual for the sound
			CreateAudioVisualForPlayerSound(audioRadius);
		}

		_groundedOnPreviousFrame = _controller.Grounded;
	}

	// uses groundInfo to determine which footstep audio to use
	private AudioClip[] FetchFootstepAudioArray() {
		if (_controller.groundInfo == null) return null;

		if (_controller.groundInfo.terrainName == "Tile") {
			return footstepsTileWalk;
		}
		if (_controller.groundInfo.terrainName == "Metal") {
			return footstepsMetalWalk;
		}

		return null;
	}

	// uses groundInfo to determine which footstep audio to use
	private AudioClip[] FetchJumpAudioArray() {
		if (_controller.groundInfo == null) return null;

		if (_controller.groundInfo.terrainName == "Tile") {
			return JumpTile;
		}
		if (_controller.groundInfo.terrainName == "Metal") {
			return JumpMetal;
		}

		return null;
	}

	// checks if player footsteps are too loud and sets player loss accordingly
	private void CheckLossByLoudAudio(float audioRadius) {
		if (playerLossByLoudFootstep && !_controller.PlayerLoss && audioRadius > playerLossByAudioThreshold) {
			string lossReason = "You made too much noise";
			string hint = "Move quietly by holding CTRL across loud surfaces to avoid making too much noise";
			_controller.OnMissionFail(lossReason, hint);
		}
	}

	// creates a sound sphere around player whenever they make a noise
	private void CreateSoundSphere(float scale) {
		// create new sounds sphere 
		GameObject newSoundSphere = Instantiate(SoundSpherePrefab);
		newSoundSphere.name = "Player Sound Sphere " + (_newSphereList.Count + 1);
		newSoundSphere.tag = "PlayerSoundSphere";

		// set transform details
		newSoundSphere.transform.localScale = new Vector3(scale, scale, scale);
		newSoundSphere.transform.position = _controller.gameObject.transform.position;

		// add sound sphere to list
		_newSphereList.Add(newSoundSphere);
	}

	// destroys all sound spheres that have lasted 1 frame and moves new spheres into the old list
	private void ClearOldSoundSpheres() {
		foreach (GameObject sphere in _oldSphereList) {
			Destroy(sphere);
		}

		_oldSphereList = _newSphereList;
		_newSphereList = new List<GameObject>();
	}

	// takes direction that sound is coming from (from player's perspective) and maps in to the 
	// perimeter of the UI canvas
	private Vector3 MapSignedAngleToScreenPerimeter(float signedAngle) {
		Vector3 perimeterVector = new Vector3(0f, 0f, 0f);

		// calculate x
		if (signedAngle >= 45f && signedAngle <= 135f) perimeterVector.x = 0f; // set visual to left side of screen
		else if (signedAngle <= -45f && signedAngle >= -135f) perimeterVector.x = Screen.width; // set visual to right side of screen
		else if (signedAngle > -45f && signedAngle < 45f) {
			float tempAngle = 90 - (signedAngle + 45f); // make the angle mapped between 0 and 90 for easier computation
			perimeterVector.x = tempAngle * Screen.width / 90f;
		}
		else if (signedAngle > 135f) {
			float tempAngle = signedAngle - 135f; // make the angle mapped between 0 and 45 for easier computation
			perimeterVector.x = tempAngle * Screen.width / 90f;
		}
		else { // signedAngle < -135
			float tempAngle = signedAngle + 180f; // make the angle mapped between 0 and 45 for easier computation
			perimeterVector.x = (tempAngle * Screen.width / 90f) + Screen.width / 2f;
		}

		// calculate y
		if (signedAngle >= 135f || signedAngle <= -135f) perimeterVector.y = 0f; // set visual to bottom side of screen
		else if (signedAngle <= 45f && signedAngle >= -45f) perimeterVector.y = Screen.height; // set visual to top side of screen
		else if (signedAngle > 45f && signedAngle < 135f) {
			float tempAngle = 90f - (signedAngle - 45f); // make the angle mapped between 0 and 90 (top to bottom) for easier computation 
			perimeterVector.y = (tempAngle * Screen.height / 90f);
		}
		else { // signedAngle < -45 && signedAngle > -135
			float tempAngle = signedAngle + 135f; // make the angle mapped between 0 and 90 (bottom to top) for easier computation 
			perimeterVector.y = (tempAngle * Screen.height / 90f);
		}

		return perimeterVector;
	}

	// sends out rays to determine if there are obstructions around the player and update feel texts
	private void UpdateFeelText() {
		int layerMask = LayerMask.GetMask("Ground", "Obstruction");
		RaycastHit hit;

		// feel in FRONT of player
		if (Physics.Raycast(transform.position, transform.forward, out hit, 5f, layerMask)) {
			// set the name
			FeelTextTopObject.GetComponent<TextMeshProUGUI>().text = hit.transform.gameObject.GetComponent<Obstruction>().AudioVisualName;

			// set the alpha based on distance
			float alpha = 255f;
			if (hit.distance > 0f) alpha = 26 + (alpha - (51f * hit.distance));
			Color newColor = new Color32(255,255,255,(byte) (alpha * _controller.VolumeRatio));
			FeelTextTopObject.GetComponent<TextMeshProUGUI>().color = newColor;
		} else {
			FeelTextTopObject.GetComponent<TextMeshProUGUI>().text = "";
		}

		// feel in BACK of player
		if (Physics.Raycast(transform.position, transform.forward * -1f, out hit, 5f, layerMask)) {
			// set the name
			FeelTextBottomObject.GetComponent<TextMeshProUGUI>().text = hit.transform.gameObject.GetComponent<Obstruction>().AudioVisualName;

			// set the alpha based on distance
			float alpha = 255f;
			if (hit.distance > 0f) alpha = 26 + (alpha - (51f * hit.distance));
			Color newColor = new Color32(255,255,255,(byte) (alpha * _controller.VolumeRatio));
			FeelTextBottomObject.GetComponent<TextMeshProUGUI>().color = newColor;
		} else {
			FeelTextBottomObject.GetComponent<TextMeshProUGUI>().text = "";
		}

		// feel to RIGHT of player
		if (Physics.Raycast(transform.position, transform.right, out hit, 5f, layerMask)) {
			// set the name
			FeelTextRightObject.GetComponent<TextMeshProUGUI>().text = hit.transform.gameObject.GetComponent<Obstruction>().AudioVisualName;

			// set the alpha based on distance
			float alpha = 255f;
			if (hit.distance > 0f) alpha = 26 + (alpha - (51f * hit.distance));
			Color newColor = new Color32(255,255,255,(byte) (alpha * _controller.VolumeRatio));
			FeelTextRightObject.GetComponent<TextMeshProUGUI>().color = newColor;
		} else {
			FeelTextRightObject.GetComponent<TextMeshProUGUI>().text = "";
		}

		// feel to LEFT of player
		if (Physics.Raycast(transform.position, transform.right * -1f, out hit, 5f, layerMask)) {
			// set the name
			FeelTextLeftObject.GetComponent<TextMeshProUGUI>().text = hit.transform.gameObject.GetComponent<Obstruction>().AudioVisualName;

			// set the alpha based on distance
			float alpha = 255f;
			if (hit.distance > 0f) alpha = 26 + (alpha - (51f * hit.distance));
			Color newColor = new Color32(255,255,255,(byte) (alpha * _controller.VolumeRatio));
			FeelTextLeftObject.GetComponent<TextMeshProUGUI>().color = newColor;
		} else {
			FeelTextLeftObject.GetComponent<TextMeshProUGUI>().text = "";
		}
	}

	// shows or hides audio panel based on if audio mode is active
	private void ShowHideAudioPanel() {

		// implement the alpha to the audio panel
		Color newColor = AudioModePanelGameObject.GetComponent<Image>().color;
		newColor.a = _controller.VolumeRatio;
		AudioModePanelGameObject.GetComponent<Image>().color = newColor;

		// implement alpha to the head slider camera image
		newColor = HeadVerticaSliderObject.GetComponent<Slider>().colors.normalColor;
		newColor.a = _controller.VolumeRatio;
		ColorBlock newColorBlock = HeadVerticaSliderObject.GetComponent<Slider>().colors;
		newColorBlock.normalColor = newColor;
		HeadVerticaSliderObject.GetComponent<Slider>().colors = newColorBlock;

		// implement alpha to the head slider background
		newColor = HeadVerticaSliderObject.transform.GetChild(0).gameObject.GetComponent<Image>().color;
		newColor.a = _controller.VolumeRatio;
		HeadVerticaSliderObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = newColor;

		// implement alpha to the crosshair image
		newColor = FocusCrosshairObject.GetComponent<Image>().color;
		newColor.a = 1f - _controller.VolumeRatio;
		FocusCrosshairObject.GetComponent<Image>().color = newColor;
	}

	private void CreateAudioVisualForPlayerSound(float audioRadius) {
		if ( _controller.VolumeRatio <= 0.667f) return;

		// instantiate new visual
		GameObject newAudioVisual = Instantiate(AudioVisualParticleSystemObjectPrefab);
		newAudioVisual.name = "player audio visual";
		newAudioVisual.AddComponent<AudioVisual>();

		// modify particle system based on soundsphere info
		ParticleSystem audioVisualParticleSystem = newAudioVisual.GetComponent<ParticleSystem>();
		var mainPS = audioVisualParticleSystem.main;

		// Set size and color
		mainPS.startSize = audioRadius * _controller.VolumeRatio / 3f;
		mainPS.startColor = Color.grey;

		// Set lifetime and speed based on timescale
		newAudioVisual.GetComponent<AudioVisual>().Lifetime = mainPS.startLifetime.constant / _controller.Timescale;
		mainPS.simulationSpeed = 6f * _controller.Timescale;

		// transform screen positions to world positions
		Vector3 centerParameters = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
		Vector3 audioVisualNewWorldPosition = _uiCamera.ScreenToWorldPoint(centerParameters);
		newAudioVisual.transform.position = audioVisualNewWorldPosition;
		newAudioVisual.transform.localPosition = new Vector3(audioVisualNewWorldPosition.x, audioVisualNewWorldPosition.y, 0f);
		newAudioVisual.transform.SetParent(AudioVisualParentObject.transform);
	}
}