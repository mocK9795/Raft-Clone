using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class Player : ObjectData
{
	[Header("Player Statistics")]
	public float range;
	public float yoinkRange;
	public float jumpHeight;
	public float swimJumpHeight;
	public float speed;
	public float swimSpeed;
	
	[Header("Object Masks")]
	public LayerMask yoinkableObjects;
	public LayerMask waterMask;
	public int selectedLayer;
	public int interactableLayer;
	
	[Header("Interaction")]
	public Raft raft;
	public GameObject buildObject;
	public float boatLeaveVelocity = 2f;
	
	Vector2 moveDirection;
	PlayerCamera playerCamera;
	ObjectData lastSelectedObject;
	Interactable lastInteractable;
	Console console;
	
	[HideInInspector]
	public bool isBuilding;
	bool isJumping;
	float currentSpeed;

	bool boatInteracter;
	bool usingHook;

	public void OnMove(InputAction.CallbackContext value)
	{
		moveDirection = value.ReadValue<Vector2>();
	}

	private new void Update()
	{
		if (isInWater)
		{
			if (isJumping) velocity.y += swimJumpHeight * Time.deltaTime;
			currentSpeed = swimSpeed;
		}
		else if (controller.isGrounded)
		{
			if (isJumping)
			{
				velocity.y = jumpHeight;
			}
			currentSpeed = speed;
		}
		if (boatInteracter)
		{
			currentSpeed = lastInteractable.bonus;
		}
		velocity += (transform.right * moveDirection.x + transform.forward * moveDirection.y) * currentSpeed * Time.deltaTime;
			
		base.Update();
	}

	public void OnJump(InputAction.CallbackContext value)
	{
		isJumping = !value.canceled;
	}

	public void OnInteract()
	{
		if (lastInteractable != null)
		{
			if (boatInteracter)
			{
				lastInteractable.transform.parent = transform.parent;
				velocity += Vector3.up * jumpHeight * boatLeaveVelocity;
			}

			if (usingHook)
			{
				lastSelectedObject.enabled = true;
				lastInteractable.transform.parent = transform.parent;
				lastSelectedObject.gameObject.layer = interactableLayer;
			}
		}
		isBuilding = false;
		boatInteracter = false;
		usingHook = false;
		lastInteractable = null;

		int lastSelectedId = -1;
		if (lastSelectedObject != null)
		{
			lastSelectedObject.UnOutline();
			lastSelectedId = lastSelectedObject.GetInstanceID();
		}
		lastSelectedObject = null;

		if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, range, yoinkableObjects)) return;
		ObjectData selectedObject = hitInfo.collider.GetComponent<ObjectData>();
		if (selectedObject == null) return;
		if (selectedObject.GetInstanceID() == lastSelectedId) return;

		selectedObject.Outline();
		lastSelectedObject = selectedObject;

		if (Raft.IsRaftComponent(selectedObject))
		{
			isBuilding = true;
			return;
		}

		Interactable interacter = selectedObject.GetComponent<Interactable>();
		if (interacter == null) return;

		lastInteractable = interacter;

		if (interacter.interacterType == Interactable.InteractionType.Boat)
		{
			boatInteracter = true;
			transform.position = interacter.transform.position;
			transform.position += interacter.offset;
			interacter.transform.parent = transform;
		}

		if (interacter.interacterType == Interactable.InteractionType.Hook)
		{
			usingHook = true;
			selectedObject.gameObject.layer = selectedLayer;
			selectedObject.enabled = false;
			interacter.transform.parent = transform;
			Vector3 interacterRotation = interacter.transform.eulerAngles;
			interacter.transform.localRotation = Quaternion.Euler(interacterRotation.x, 0, interacterRotation.z);
			interacter.transform.localPosition = interacter.offset;
		}

	}

	public void OnInteract(InputAction.CallbackContext value) { if (value.canceled) OnInteract(); }

	public void OnAttack()
	{
		if (isBuilding)
		{
			if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, yoinkRange, waterMask)) return;
			print(rayInfo.collider.name);
			GameObject builtObject = Instantiate(buildObject);
			builtObject.transform.position = rayInfo.point;
			bool success = raft.AddComponent(builtObject);
			if (!success) Destroy(builtObject);
			return;
		}
		else if (usingHook)
		{
			if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, yoinkRange + lastInteractable.range, yoinkableObjects)) return;
			ObjectData hookedObject = rayInfo.collider.GetComponent<ObjectData>();
			print(hookedObject.name);

			if (hookedObject == null) return;
			if (hookedObject.withinParent) return;

			hookedObject.velocity += (transform.position - hookedObject.transform.position) * lastInteractable.bonus;

			return;
		}

		if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, yoinkRange, yoinkableObjects)) return;
		print(hitInfo.collider.name);
		ObjectData selectedObject = hitInfo.collider.GetComponent<ObjectData>();

		if (selectedObject == null) return;
		if (selectedObject.withinParent) return;

		inventory.Add(selectedObject.inventory);
		console.Message("Picked Up " + hitInfo.collider.name);
		Destroy(selectedObject.gameObject);
	}

	public void OnAttack(InputAction.CallbackContext value) {if (value.canceled) OnAttack();}

	private new void Start()
	{
		base.Start();
		playerCamera = FindAnyObjectByType<PlayerCamera>();
		controller = GetComponent<CharacterController>();
		console = FindFirstObjectByType<Console>();
		interact = true;
	}
}
