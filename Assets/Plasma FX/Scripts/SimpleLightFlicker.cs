using UnityEngine;

public class SimpleLightFlicker : MonoBehaviour {
	public AnimationCurve intensityCurve;
	public float speed = 1f;
	public float curveMultiplier = 1f;
	Light ulight;

	float fadeIn = 0f;

	void Start () {
		ulight = GetComponent<Light>();
		intensityCurve.postWrapMode = WrapMode.Loop;
	}
	
	void Update () {
		if(fadeIn > 1){
			ulight.intensity = intensityCurve.Evaluate(Time.time * speed) * curveMultiplier;
		} else {
			fadeIn += Time.deltaTime *.5f;
			ulight.intensity = intensityCurve.Evaluate(Time.time * speed) * curveMultiplier * Mathf.Clamp01(fadeIn);
		}
	}
}
