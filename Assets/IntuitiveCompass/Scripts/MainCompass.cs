using UnityEngine;
using UnityEngine.UI;

public class MainCompass : MonoBehaviour {
	
	public Vector3 NorthDestination;
	public Vector3 TargetDestination;
	public bool ObjectInstead = true;
	public GameObject Target;
	
	public GameObject CompassArrows;
	public bool DrawDebugRay = false;
	public bool ShowDegrees = false;
	public bool DegreesUseSetNorth = false;
	public Text DegreesObject;
	
	private float PrevRotation;
	private float NewRotation;
	private float CompassRotation;
	private Vector3 RelPos;
	private Vector2 RelPosXZ;
	private Vector2 RayDirXZ;
	private Vector3 CamDir;
	//private Vector3 Fix;
	private Vector3 normal;
	private Vector3 RelPosXYZ;
	private Vector3 RayDirXYZ;
	private Image CompassImage;
	private Image ArrowsImage;
	private bool ErrorFounded = false;
	
	private Transform CompassParent;
	
	void Start() {
		//Fix = new Vector3(367, 209, 0);
		CompassParent = CompassArrows.transform.parent;
		CompassImage = CompassParent.gameObject.GetComponent<Image>();
		ArrowsImage = CompassArrows.GetComponent<Image>();
	}
	
	public void RotateCompass(Ray ray) {
		RelPos = TargetDestination - Camera.main.transform.position;
		RelPosXZ = new Vector2(RelPos.x, RelPos.z);
		RayDirXZ = new Vector2(ray.direction.x, ray.direction.z);
		
		
		RelPosXYZ = new Vector3(RelPos.x, RelPos.z, 0);
		RayDirXYZ = new Vector3(ray.direction.x, ray.direction.z, 0);
		
		Vector3 normal = Vector3.Cross(RelPosXYZ, RayDirXYZ);
		
		if (normal.z < 0) {
			NewRotation = Vector2.Angle(RelPosXZ, RayDirXZ);
		} else {
			NewRotation = 360 - Vector2.Angle(RelPosXZ, RayDirXZ);
		}
		
		if (PrevRotation != NewRotation && ErrorFounded == false) {
			CompassRotation = Mathf.Abs(NewRotation) - Mathf.Abs(PrevRotation);
			PrevRotation = NewRotation;

			// point the arrow towards the object
			if (CompassArrows != null) {
				CompassArrows.transform.Rotate(Vector3.forward, CompassRotation);
			} else {
				ErrorFounded = true;
				Debug.Log("Object 'arrows' is not assigned");
			}
		}

		if (DegreesObject != null) {
			DegreesObject.text = Mathf.Round(NewRotation).ToString();
		}
	}

	public void UpdateDegreesSeperately(Ray ray) {
		if (ShowDegrees == true && ErrorFounded == false) {
			float degreeRotation = 0f;
			if (DegreesObject != null) {
				// set degrees number based on where north is (not on object)
				if (DegreesUseSetNorth) {
					RelPos = NorthDestination - Camera.main.transform.position;
					RelPosXZ = new Vector2(RelPos.x, RelPos.z);
					RayDirXZ = new Vector2(ray.direction.x, ray.direction.z);

					RelPosXYZ = new Vector3(RelPos.x, RelPos.z, 0);
					RayDirXYZ = new Vector3(ray.direction.x, ray.direction.z, 0);

					normal = Vector3.Cross(RelPosXYZ, RayDirXYZ);

					if (normal.z < 0) degreeRotation = Vector2.Angle(RelPosXZ, RayDirXZ);
					else degreeRotation = 360f - Vector2.Angle(RelPosXZ, RayDirXZ);
				}
				DegreesObject.text = Mathf.Round(degreeRotation).ToString();
			} else {
				ErrorFounded = true;
				Debug.Log("No object is selected to display the number of degrees");
			}
		}
	}
	
	void Update() {
		if (ObjectInstead && ErrorFounded == false) {
			if (Target == null) {
				ErrorFounded = true;
				Debug.Log("Object 'north' is not assigned");
			}
			TargetDestination = Target.transform.position;
		}
		if (ErrorFounded == false) {
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
			if (DrawDebugRay) {
				Debug.DrawRay(ray.origin, ray.direction*200, Color.black);
			}
		
			RotateCompass(ray);
			//UpdateDegreesSeperately(ray);
		}
	}
}
