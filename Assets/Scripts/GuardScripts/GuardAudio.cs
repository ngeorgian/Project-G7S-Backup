using UnityEngine;
using System.Collections.Generic;
using System;

public class GuardAudio : MonoBehaviour {
	/******* PUBLIC VARIABLES *******/
	[Header("Tile")]
	[Tooltip("Walking footsteps for tile")]
	public AudioClip[] footstepsTileWalk;

	[Space(10)]
	[Header("Metal")]
	[Tooltip("Walking footsteps for metal")]
	public AudioClip[] footstepsMetalWalk;

	[Space(10)]
	[Header("Sound Sphere")]
	public GameObject SoundSpherePrefab;

	/******* PRIVATE VARIABLES *******/
	private float _footstepAudioCooldown;

	// create 2 lists for holding sound spheres ->
	// newSphereList holds sounds sphere references ->
	// at the end of every frame, old spheres are destroyed ->
	// new spheres become old spheres
	private List<GameObject> _newSphereList = new List<GameObject>();
	private List<GameObject> _oldSphereList = new List<GameObject>();

	// external component references
	// holds reference to the player controller script
    private PlayerController _controller;
	private GuardStateMachine _stateMachine;
	private UnityEngine.AI.NavMeshAgent _agent;

	private void Awake() {
		_stateMachine = GetComponent<GuardStateMachine>();
		_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

		_controller = _stateMachine.PlayerGameObject.GetComponent<PlayerController>();
	}

	private void FixedUpdate() {
		GetComponent<AudioSource>().pitch = _controller.Timescale;

		PlayFootsteps();
		ClearOldSoundSpheres();
	}

	// check when guard collides with trigger collider
    private void OnTriggerEnter(Collider other) {
        // if the collided object is a sound sphere
        if (other.gameObject.tag == "PlayerSoundSphere" && _stateMachine.IsPlayerInSameLevel()) {
            // calculate distance between guard and player
            float distance = Vector3.Distance(transform.position, _stateMachine.PlayerGameObject.transform.position);

            // calculate difference between distance and radius of sound sphere (radius = scale of sphere)
            float radius = other.gameObject.transform.localScale.x;
            float difference = radius - distance;
            if (difference <= 0f) return;

            // raycast between guard and player and count number of obstructions between them
            int layerMask = (1<<8); // only count objects in the 'obstruction' layer (layer 8)
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, (_stateMachine.PlayerGameObject.transform.position - transform.position), distance, layerMask);

            // awarness += (distance between guard and player * 0.7 ) divided by (1 + the number of obstruction objects between guard and player)
            _stateMachine.Awareness += (difference * 0.7f) / (1 + hits.Length);

            // check if guard should pursuit player
            _stateMachine.CheckPursuit();
        }
    }

	private void PlayFootsteps() {
		if (_footstepAudioCooldown > 0f) {
			_footstepAudioCooldown -= Time.fixedDeltaTime;
		}

		if (_agent.velocity.magnitude > 0.85f && _footstepAudioCooldown <= 0f) {
			// determine which audioArray to use based on groundInfo
			AudioClip[] audioArray = FetchFootstepAudioArray();

			// pick random footstep audioClip to play
			int index = UnityEngine.Random.Range (0, audioArray.Length);
			AudioClip footstep = audioArray[index];

			// calculate distance between guard and player
			float distance = Vector3.Distance(transform.position, _stateMachine.PlayerGameObject.transform.position);

			// raycast between guard and player and count number of obstructions between them
            int layerMask = (1<<8); // only count objects in the 'obstruction' layer (layer 8)
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, (_stateMachine.PlayerGameObject.transform.position - transform.position), distance, layerMask);

			// count number of full obstructions
			int fullObstructionCount = 0;
			float noiseReduction = 0f;
			foreach (RaycastHit hit in hits) {
				if (hit.collider.gameObject.GetComponent<Obstruction>() == null) {
					Debug.Log("No Obstruction script found for obstruction object: " + hit.collider.gameObject.name);
					continue;
				}
				if (hit.collider.gameObject.GetComponent<Obstruction>().FullObstruction) fullObstructionCount++;
				noiseReduction += hit.collider.gameObject.GetComponent<Obstruction>().NoiseReduction;
			}

			// play audio and set cooldown
			float volumeRatio = Math.Max(Math.Min((3f *  _controller.VolumeRatio), 3f) - 2f, 0f);
			_footstepAudioCooldown = 0.6f;
			float audioRadius = _stateMachine.GroundInfo.WalkRadius;
			GetComponent<AudioSource>().volume = 1f / (float)Math.Pow(2, noiseReduction * hits.Length * volumeRatio);
			GetComponent<AudioSource>().maxDistance = audioRadius;
			GetComponent<AudioSource>().PlayOneShot(footstep, _stateMachine.GroundInfo.WalkNoise * 2f * volumeRatio);

			// create sound sphere
			CreateSoundSphere(audioRadius * 2, GetComponent<AudioSource>().volume);
		}
	}

	// uses groundInfo to determine which footstep audio to use
	private AudioClip[] FetchFootstepAudioArray() {
		if (_stateMachine.GroundInfo == null) return null;

		if (_stateMachine.GroundInfo.terrainName == "Tile") {
			return footstepsTileWalk;
		}
		if (_stateMachine.GroundInfo.terrainName == "Metal") {
			return footstepsMetalWalk;
		}

		return null;
	}

	// creates a sound sphere around guard whenever they make a noise
	private void CreateSoundSphere(float scale, float volume) {
		// create new sounds sphere 
		GameObject newSoundSphere = Instantiate(SoundSpherePrefab);
		newSoundSphere.name = gameObject.name + " Sound Sphere " + (_newSphereList.Count + 1);
		newSoundSphere.tag = "EnemySoundSphere";

		newSoundSphere.AddComponent<SoundSphere>();
		newSoundSphere.GetComponent<SoundSphere>().GroundInfo = _stateMachine.GroundInfo;
		newSoundSphere.GetComponent<SoundSphere>().volume = volume;
		newSoundSphere.GetComponent<SoundSphere>().audioVisualColor = Color.red;
		newSoundSphere.GetComponent<SoundSphere>().audioFocus = false;
		newSoundSphere.GetComponent<SoundSphere>().SoundEmitterGameObject = this.gameObject;

		// set transform details
		newSoundSphere.transform.localScale = new Vector3(scale, scale, scale);
		newSoundSphere.transform.position = gameObject.transform.position;

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
}