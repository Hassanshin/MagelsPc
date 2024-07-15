using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnTriggerEnterEvent : MonoBehaviour
{
	public UnityEvent<Collider> TriggerEnter = new UnityEvent<Collider>();
	public bool IsDestroyAfterTrigger;
	protected virtual void OnTriggerEnter(Collider other)
	{
		TriggerEnter?.Invoke(other);
		
		if (IsDestroyAfterTrigger)
		{
			Destroy(this.gameObject);
		}
	}
}
