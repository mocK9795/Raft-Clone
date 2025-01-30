using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType {Boat};
    public InteractionType interacterType;
    public Vector3 offset;
    public float speedBonus;
}
