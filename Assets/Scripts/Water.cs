using UnityEngine;

public class Water : MonoBehaviour
{
	public float floatOffset;
	public float density;
	public Vector2 flowDirection;
	public Vector2 raftFlowDirection;
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
			floatingObject.velocity += (new Vector3(raftFlowDirection.x, 0, raftFlowDirection.y)
				* Time.deltaTime) / floatingObject.mass;
		}
		else
		{
			floatingObject.velocity += (new Vector3(flowDirection.x, 0, flowDirection.y) 
				* Time.deltaTime) / floatingObject.mass;
		}

		float boyancyProvided = density * data.gravity * ((transform.position.y + floatOffset) - floatingObject.transform.position.y) * Time.deltaTime;
		floatingObject.velocity.y += boyancyProvided;
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
