using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Console : MonoBehaviour
{
    public float messageShowTime;
    public GameObject messageContainer;
    public TMP_Text messagePreset;
    Queue<TMP_Text> messages = new Queue<TMP_Text>();
    float removeMessageTimer;

    public void Message(string msg)
    {
        TMP_Text text = Instantiate(messagePreset, messageContainer.transform);
        text.text = msg;

        messages.Enqueue(text);
    }

	public void Update()
	{
        if (messages.Count < 1) return;

        removeMessageTimer += Time.deltaTime;

        if (removeMessageTimer > messageShowTime)
        {
            removeMessageTimer = 0;

            TMP_Text message = messages.Dequeue();
            Destroy(message);
        }
	}
}
