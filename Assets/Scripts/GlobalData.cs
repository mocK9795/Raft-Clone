using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public float gravity;
    public float friction;
    public Vector3 heldItemOffset;
    public Material outlineMaterial;
    public Material nullMaterial;
    public GameObject water;
    public Transform target;

	private void Update()
	{
        water.transform.position = new Vector3(target.position.x, 0, target.position.z);
	}
}
