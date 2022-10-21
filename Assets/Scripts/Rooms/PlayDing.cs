using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlayDing : MonoBehaviour {
    /******* PUBLIC VARIABLES *******/
    [Tooltip("Holds reference to the player object")]
    public GameObject PlayerGameObject;
    [Tooltip("Amount of time between playing the audio")]
    public float NoiseCooldownTime;
    [Tooltip("Holds reference to the next sound emitter object to activate once player gets close")]
    public GameObject NextSoundEmitterObject;
    [Tooltip("Holds reference to the UI GameObject")]
    public GameObject FollowDingsGameObject;
    [Tooltip("Holds reference to the new text for FollowDingText")]
    public string NewFollowDingText;

    [Space(10)]
	[Header("Sound Sphere")]
	public GameObject SoundSpherePrefab;

    /******* PRIVATE VARIABLES *******/
    private float _noiseCooldown;
    private float _playCutoffCooldownTime = 0.35f;
    private float _playCutoffCooldown;

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

    private void FixedUpdate(){
        PlayDingSound();
        ClearOldSoundSpheres();
        CheckPlayerDistance();
    }

    // creates a sound sphere around guard whenever they make a noise
	private void CreateSoundSphere(float scale, float volume) {
		// create new sounds sphere 
		GameObject newSoundSphere = Instantiate(SoundSpherePrefab);
		newSoundSphere.name = gameObject.name + " Sound Sphere " + (_newSphereList.Count + 1);
		newSoundSphere.tag = "EnvironmentSoundSphere";

		newSoundSphere.AddComponent<SoundSphere>();
		newSoundSphere.GetComponent<SoundSphere>().GroundInfo = null;
		newSoundSphere.GetComponent<SoundSphere>().volume = volume;
        newSoundSphere.GetComponent<SoundSphere>().audioVisualColor = Color.blue;

		// set transform details
		newSoundSphere.transform.localScale = new Vector3(scale, scale, scale);
		newSoundSphere.transform.position = gameObject.transform.position;

		// add sound sphere to list
		_newSphereList.Add(newSoundSphere);
	}

    // plays the sound and updates cooldown
    private void PlayDingSound() {
        _noiseCooldown += Time.deltaTime;

        if(_noiseCooldown >= NoiseCooldownTime && _controller.VolumeRatio > 0.75) {
            GetComponent<AudioSource>().volume = 1f;
            CreateSoundSphere(80f, 1f);
            GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
            _noiseCooldown = 0f;
            _playCutoffCooldown = 0f;
        }

        if(_playCutoffCooldown < _playCutoffCooldownTime) {
            _playCutoffCooldown += Time.deltaTime;

            if(_playCutoffCooldown >= _playCutoffCooldownTime) {
                GetComponent<AudioSource>().volume = 0f;
            }
        }
    }

    // If player is close to sound emitter, turn on the next sound emitter
    private void CheckPlayerDistance() {
        if (Vector3.Distance(gameObject.transform.position, PlayerGameObject.transform.position) < 7.5f) {
            StartCoroutine(WaitForDingToEnd());
            FollowDingsGameObject.GetComponent<TMP_Text>().text = NewFollowDingText;
        }
    }

    IEnumerator WaitForDingToEnd() {
        while(GetComponent<AudioSource>().volume > 0f) yield return new WaitForSeconds(Time.deltaTime);
        if (NextSoundEmitterObject != null) NextSoundEmitterObject.SetActive(true);
        gameObject.SetActive(false);
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