using System;
using UnityEngine;

public class LoadSceneData : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of the scene to load")]
    public string SceneName;

	[SerializeField]
	[Tooltip("Shared scene between platforms")]
	public bool SharedScene = false;

    [SerializeField]
    [Tooltip("Start scene in Admin Mode?")]
    public bool IsAdmin;
}
