using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EffectsController : MonoBehaviour
{
    [SerializeField] ScriptableRendererFeature underwaterEffect;
	public Transform target;
	public Transform water;

	private void Start()
	{
		underwaterEffect.SetActive(false);
	}

	private void Update()
	{
		if (target.position.y <= water.position.y)
		{
			underwaterEffect.SetActive(true);
		}
		else
		{
			underwaterEffect.SetActive(false);
		}
	}
}
