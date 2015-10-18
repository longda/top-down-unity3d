using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour 
{
	Vector3 velocity;
	Rigidbody rb;
	
	void Start() 
	{
		rb = GetComponent<Rigidbody>();
	}
	
	public void Move(Vector3 velocity)
	{
		this.velocity = velocity;
	}
	
	public void LookAt(Vector3 lookPoint)
	{
		var heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt(heightCorrectedPoint);
	}
	
	private void FixedUpdate()
	{
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}
}
