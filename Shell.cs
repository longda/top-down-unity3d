using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour 
{
	public Rigidbody myRigidBody;
	public float forceMin;
	public float forceMax;
	
	float lifetime = 4f;
	float fadetime = 2f;
	
	void Start () 
	{
		var force = Random.Range(forceMin, forceMax);
		myRigidBody.AddForce(transform.right * force);
		myRigidBody.AddTorque(Random.insideUnitSphere * force);
		
		StartCoroutine(Fade());
	}
	
	IEnumerator Fade()
	{
		yield return new WaitForSeconds(lifetime);
		
		var percent = 0f;
		var fadeSpeed = 1 / fadetime;
		var mat = GetComponent<Renderer>().material;
		var initialColor = mat.color;
		
		while (percent < 1)
		{
			percent += Time.deltaTime * fadeSpeed;
			mat.color = Color.Lerp(initialColor, Color.clear, percent);
			yield return null;
		}
		
		Destroy(gameObject);
	}
}
