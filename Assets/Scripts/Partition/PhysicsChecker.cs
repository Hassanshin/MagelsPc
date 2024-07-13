using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsChecker : MonoBehaviour
{
	[SerializeField]
	float radius = 1;
	[SerializeField]
	LayerMask obs;
	
	public bool yes;
	
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		yes = Physics.CheckSphere(this.transform.position, radius, obs);
	}
}
