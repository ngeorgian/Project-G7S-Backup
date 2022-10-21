using UnityEngine;
using System.Collections.Generic;
using System;

public class TripwireAudio : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Reference to Player gameobject")]
    public GameObject PlayerGameObject;
    [Header("Sound Sphere")]
	public GameObject SoundSpherePrefab;

    [Header("Audio Clip")]
	public AudioClip TripWireSoundClip;
    public float AudioRadius = 30f;
	[Tooltip("Multiplier for how loud the volume of the tripwire is")]
	public float SoundAdjustment = 1f;

    /******* PRIVATE VARIABLES *******/
    private float _audioCooldown = 0f;

    // create 2 lists for holding sound spheres ->
	// newSphereList holds sounds sphere references ->
	// at the end of every frame, old spheres are destroyed ->
	// new spheres become old spheres
	private List<GameObject> _newSphereList = new List<GameObject>();
	private List<GameObject> _oldSphereList = new List<GameObject>();

    // external component references
	// holds reference to the player controller script
    private PlayerController _controller;

    private void Awake() {
        _controller = PlayerGameObject.GetComponent<PlayerController>();
    }

    private void FixedUpdate() {
		GetComponent<AudioSource>().pitch = _controller.Timescale;

		EmitSoundSphere();
		ClearOldSoundSpheres();
	}

    private void EmitSoundSphere() {
        if(_audioCooldown >= 0f) {
            _audioCooldown -= Time.fixedDeltaTime;
            return;
        }

        // calculate distance between guard and player
			float distance = Vector3.Distance(transform.position, PlayerGameObject.transform.position);

			// raycast between guard and player and count number of obstructions between them
            int layerMask = (1<<8); // only count objects in the 'obstruction' layer (layer 8)
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, (PlayerGameObject.transform.position - transform.position), distance, layerMask);

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
			_audioCooldown = TripWireSoundClip.length;
			float audioRadius = AudioRadius;
			GetComponent<AudioSource>().volume = 1f / (float)Math.Pow(2, noiseReduction * hits.Length * volumeRatio);
			GetComponent<AudioSource>().maxDistance = audioRadius;
			GetComponent<AudioSource>().PlayOneShot(TripWireSoundClip, audioRadius * volumeRatio * SoundAdjustment);

			// create sound sphere
			CreateSoundSphere(audioRadius * 2, GetComponent<AudioSource>().volume);
    }

    // creates a sound sphere around tripwire whenever it makes a noise
	private void CreateSoundSphere(float scale, float volume) {
		// create new sounds sphere 
		GameObject newSoundSphere = Instantiate(SoundSpherePrefab);
		newSoundSphere.name = gameObject.name + " Sound Sphere " + (_newSphereList.Count + 1);
		newSoundSphere.tag = "EnemySoundSphere";

		newSoundSphere.AddComponent<SoundSphere>();
		newSoundSphere.GetComponent<SoundSphere>().volume = volume;
		newSoundSphere.GetComponent<SoundSphere>().audioVisualColor = Color.yellow;
        newSoundSphere.GetComponent<SoundSphere>().audioFocus = true;
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