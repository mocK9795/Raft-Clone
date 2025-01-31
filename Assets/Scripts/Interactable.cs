using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType {Boat, Hook};
    public InteractionType interacterType;
    public Vector3 offset;
    public float bonus;
    public float range;
}
