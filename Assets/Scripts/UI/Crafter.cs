using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[CreateAssetMenu()]
public class Crafter : ScriptableObject
{
    public List<Craft> crafts = new List<Craft>();

	public Inventory GetCost(string name)
	{
		foreach (Craft craft in crafts)
		{
			if (craft.name == name)
			{
				return craft.cost;
			}
		}

		return new Inventory();
	}

	public Inventory GetCost(GameObject craftableObject)
	{
		ObjectData obj = craftableObject.GetComponent<ObjectData>();
		if (obj == null) throw (new SystemException("Non craftable object"));
		return GetCost(obj);
    }

	public Inventory GetCost(ObjectData obj)
	{
		return GetCost(obj.itemName);
	}
}

[System.Serializable]
public struct Craft 
{
	public string name;
	public Inventory cost;
}


[System.Serializable]
public struct Inventory
{
	public List<Item> items;

	public Inventory(List<Item> items) : this() { this.items = items; }
	public Inventory(Item item) : this() { items = new List<Item> { item }; }

	public Inventory(string blankCreation) {items = new List<Item>(); }

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
		foreach (Item containdedItem in items)
		{
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
		if (item.name == name) return true;
		return false;
	}

	public bool Contains(Item item)
	{
		if (item.name == name && count >= item.count) return true;
		return false;
	}
}
