using UnityEngine;

public class Water : MonoBehaviour
{
	public float floatOffset;
	public float density;
	public Vector2 flowDirection;
	[HideInInspector] public BoxCollider box;
	GlobalData data;

	private void Start()
	{
		data = FindAnyObjectByType<GlobalData>();
		box = GetComponent<BoxCollider>();
	}

	private void OnTriggerStay(Collider other)
	{
		ObjectData floatingObject = other.GetComponent<ObjectData>();
		if (floatingObject == null) return;
		if (Raft.IsRaftComponent(floatingObject))
		{
			floatingObject = floatingObject.transform.parent.GetComponent<ObjectData>();
		}

		float boyancyProvided = density * data.gravity * ((transform.position.y + floatOffset) - floatingObject.transform.position.y) * Time.deltaTime;
		floatingObject.velocity.y += boyancyProvided;
		floatingObject.velocity += (new Vector3(flowDirection.x, 0, flowDirection.y) * Time.deltaTime) / floatingObject.mass;
	}

	private void OnTriggerEnter(Collider other)
	{
		ObjectData floatingObject = other.GetComponent<ObjectData>();
		if (floatingObject == null) return;
		floatingObject.isInWater = true;
	}

	private void OnTriggerExit(Collider other)
	{
		ObjectData floatingObject = other.GetComponent<ObjectData>();
		if (floatingObject == null) return;
		floatingObject.isInWater = false;
	}
}
