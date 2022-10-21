using UnityEngine;

public class DarkTransition : MonoBehaviour {
    [Tooltip("Flag for if the environment should get darker or lighter as the player moves forward ")]
    public bool FadeToBlack;
    [Tooltip("GameObject that transitions environment back to previous state")]
    public GameObject NextDarkTransitionObject;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag != "Player") return;
        
        if(FadeToBlack) RenderSettings.ambientIntensity = 0f;
        else RenderSettings.ambientIntensity = 1f;

        if(NextDarkTransitionObject != null) {
            NextDarkTransitionObject.SetActive(true);
        }

        this.gameObject.SetActive(false);
    }
}