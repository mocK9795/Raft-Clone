using UnityEngine;
using System.Collections.Generic;

public class Raft : ObjectData
{ 
	public float cellSizeX;
	public float cellSizeZ;
	public enum MassRecalculationMode { Average, Addition};
	public MassRecalculationMode massCalculation;
	public List<ObjectData> components = new List<ObjectData>();
	List<Vector2> placedPositions = new List<Vector2>();

	public bool AddComponent(ObjectData component)
	{
		component.Start();
		
		Vector3 componentPosition = component.transform.position;
		Vector2 placementPosition = new Vector2(
			Mathf.RoundToInt(componentPosition.x / cellSizeX) * cellSizeX,
			Mathf.RoundToInt(componentPosition.z / cellSizeZ) * cellSizeZ
			);
		if (placedPositions.Contains(placementPosition)) return false;
		placedPositions.Add(placementPosition);
		component.transform.position = new Vector3(placementPosition.x, transform.position.y, placementPosition.y);
		component.withinParent = true;
		component.enabled = false;
		component.transform.parent = transform;

		if (massCalculation == MassRecalculationMode.Average) mass = (mass + component.mass) / 2f; 
		else if (massCalculation == MassRecalculationMode.Addition) mass += component.mass;
		
		components.Add(component);

		return true;
	}

	public bool AddComponent(GameObject component)
	{
		ObjectData componentData = component.GetComponent<ObjectData>();
		if (componentData == null) return false;
		return AddComponent(componentData);
	}

	private new void Start()
	{
		base.Start();

		BoxCollider componentBox = components[0].GetComponent<BoxCollider>();
		Vector3 componentSize = componentBox.size;
		Vector3 boxScale = componentBox.transform.localScale;
		cellSizeX = componentSize.x * boxScale.x;
		cellSizeZ = componentSize.z * boxScale.y;

		List<ObjectData> temporaryList = new List<ObjectData>(components);
		components.Clear();
		for (int i = 0; i < temporaryList.Count; i++)
		{
			AddComponent(temporaryList[i]);
		}
		temporaryList.Clear();
	}

	public static bool IsRaftComponent(ObjectData component)
	{
		if (!component.withinParent) return false;
		Raft raft = component.GetComponentInParent<Raft>();
		if (raft == null) return false;
		return true;
	}
}

[System.Serializable]
public struct Inventory
{
	public List<Item> items;

	public Inventory(List<Item> items) : this() {this.items = items;}
	public Inventory(Item item) : this() { items = new List<Item> { item }; }

	public bool Includes(Inventory inventory)
	{
		foreach (Item otherItem in inventory.items)
		{
			if (!Includes(otherItem)) return false;
		}
		return true;
	}
	
	public bool Includes(Item item)
	{
		foreach (Item containdedItem in items) { 
			if (containdedItem.Contains(item)) return true;
		}
		return false;
	}

	public void Add(Inventory inventory)
	{
		foreach (Item otherItem in inventory.items)
		{
			Add(otherItem);
		}
	}

	public void Add(Item item) 
	{
		for (int i = 0; i < items.Count; i++)
		{
			Item containedItem = items[i];
			if (containedItem.Equals(item))
			{
				containedItem.count += item.count;
				items[i] = containedItem;
				return;
			}
		}

		items.Add(item);
	}
}

[System.Serializable]
public struct Item
{
	public string name;
	public int count;

	public Item(string name, int count) : this()
	{
		this.name = name;
		this.count = count;
	}

	public bool Equals(Item item)
	{
		if (item.name ==  name) return true;
		return false;
	}

	public bool Contains(Item item)
	{
		if (item.name == name && count >= item.count) return true;
		return false;
	}
}