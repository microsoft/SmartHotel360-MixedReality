using System;
using UnityEngine;

public class LoadSceneData : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of the scene to load")]
    public string SceneName;

    [SerializeField]
    [Tooltip("Start scene in Admin Mode?")]
    public bool IsAdmin;
}
