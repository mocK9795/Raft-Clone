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
	public float crouchStrength;
	public float speed;
	public float swimSpeed;
	
	[Header("Object Masks")]
	public LayerMask yoinkableObjects;
	public LayerMask waterMask;
	public int selectedLayer;
	public int interactableLayer;
	List<int> itemStackLayers = new List<int>();
	
	[Header("Interaction")]
	public Raft raft;
	[HideInInspector] public List<ObjectData> itemStack = new List<ObjectData>();
	public GameObject buildObject;
	public float boatLeaveVelocity = 2f;
	int lastSelectedItemId = -1;
	
	Vector2 moveDirection;
	PlayerCamera playerCamera;
	ObjectData lastSelectedObject;
	Interactable lastInteractable;
	
	[HideInInspector]
	public bool isBuilding;
	bool isJumping;
	bool isCrouching;
	float currentSpeed;

	bool boatInteracter;
	bool usingHook;

	public void OnCrouch(InputAction.CallbackContext value) { isCrouching = !value.canceled; }
	public void OnMove(InputAction.CallbackContext value)
	{
		moveDirection = value.ReadValue<Vector2>();
	}
	public void OnJump(InputAction.CallbackContext value)
	{
		isJumping = !value.canceled;
	}
	public void OnInteract(InputAction.CallbackContext value) { if (value.canceled) OnInteract(); }
	public void OnAttack(InputAction.CallbackContext value) { if (value.canceled) OnAttack(); }

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

		lastSelectedItemId = -1;
		if (lastSelectedObject != null)
		{
			lastSelectedObject.UnOutline();
			lastSelectedItemId = lastSelectedObject.GetInstanceID();
		}
		lastSelectedObject = null;

		if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, range, yoinkableObjects)) return;
		ObjectData selectedObject = hitInfo.collider.GetComponent<ObjectData>();
		if (selectedObject == null) return;
		if (selectedObject.GetInstanceID() == lastSelectedItemId) return;
		lastSelectedItemId = selectedObject.GetInstanceID();

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

	public void OnAttack()
	{
		if (isBuilding)
		{
			if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, yoinkRange, waterMask)) return;
			Inventory stack = GetStackAsInventory();
			Inventory cost = data.crafter.GetCost(buildObject);
			if (stack.Includes(cost)) RemoveInventoryFromStack(cost);
			else return;
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

			if (hookedObject == null) return;
			if (hookedObject.withinParent) return;

			hookedObject.velocity += (transform.position - hookedObject.transform.position) * lastInteractable.bonus;

			return;
		}
		else if (boatInteracter) return;

		if (itemStack != null && isJumping && itemStack.Count > 0)
		{
			itemStack[0].gameObject.layer = itemStackLayers[0];
			itemStack[0].transform.parent = transform.parent;
			itemStack[0].enabled = true;
			itemStack[0].velocity = playerCamera.transform.forward * data.itemThrowStrength;
			itemStackLayers.RemoveAt(0);
			itemStack.RemoveAt(0);
			RestackItems();
		}
		if (isJumping && itemStack.Count > 0) return;
		
		if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, yoinkRange, yoinkableObjects)) return;

		ObjectData selectedObject = hitInfo.collider.GetComponent<ObjectData>();

		if (selectedObject == null) return;
		if (selectedObject.withinParent) return;
		if (selectedObject.gameObject.GetInstanceID() == lastSelectedItemId) return;

		selectedObject.enabled = false;
		itemStackLayers.Add(selectedObject.gameObject.layer);
		selectedObject.gameObject.layer = selectedLayer;
		selectedObject.transform.parent = transform;
		selectedObject.transform.localPosition = data.heldItemOffset;
		Vector3 selectedItemRotation = selectedObject.transform.eulerAngles;
		selectedObject.transform.localRotation = Quaternion.Euler(selectedItemRotation.x, 0, selectedItemRotation.z);
		itemStack.Add(selectedObject);

		RestackItems();
	}

	public void RestackItems()
	{
		if (itemStack.Count < 1) return;
		itemStack[0].transform.localPosition = data.heldItemOffset;
		if (itemStack.Count < 2) return;

		for (int i = 1; i < itemStack.Count; i++)
		{
			ObjectData prevItem = itemStack[i-1];
			Collider prevCollider = prevItem.GetComponent<Collider>();
			Vector3 prevItemSize = prevCollider.bounds.extents;
			float prevItemHeight = prevItemSize.y * Mathf.Abs(prevItem.transform.up.y) +
						   prevItemSize.x * Mathf.Abs(prevItem.transform.up.x) +
						   prevItemSize.z * Mathf.Abs(prevItem.transform.up.z);
			ObjectData item = itemStack[i];
			Collider itemCollider = item.GetComponent<Collider>();
			Vector3 itemSize = itemCollider.bounds.extents;
			float itemHeight = itemSize.y * Mathf.Abs(item.transform.up.y) +
						   itemSize.x * Mathf.Abs(item.transform.up.x) +
						   itemSize.z * Mathf.Abs(item.transform.up.z);
			item.transform.localPosition = prevItem.transform.localPosition + Vector3.up * prevItemHeight + Vector3.up * itemHeight;
		}
	}

	public Inventory GetStackAsInventory()
	{
		Inventory inventory = new Inventory("");

		foreach (ObjectData item in itemStack)
		{
			Item newItem = new Item(item.itemName, 1);
			inventory.Add(newItem);
		}
		return inventory;
	}

	public void RemoveInventoryFromStack(Inventory inventory)
	{
		foreach (Item item in inventory.items)
		{
			for (int i = 0; i < item.count; i++) 
			{
				for (int stackIndex = 0; stackIndex < itemStack.Count; stackIndex++)
				{
					if (itemStack[stackIndex].itemName == item.name)
					{
						Destroy(itemStack[stackIndex].gameObject);
						itemStack.RemoveAt(stackIndex);
						itemStackLayers.RemoveAt(stackIndex);
						break;
					}
				}
			}
		}
	}

	private new void Start()
	{
		base.Start();
		playerCamera = FindAnyObjectByType<PlayerCamera>();
		controller = GetComponent<CharacterController>();
		interact = true;
	}

	private new void Update()
	{
		if (isInWater)
		{
			if (isJumping) velocity.y += swimJumpHeight * Time.deltaTime;
			if (isCrouching) velocity.y -= crouchStrength * Time.deltaTime;
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

}
