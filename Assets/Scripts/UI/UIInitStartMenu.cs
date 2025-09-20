using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInitStartMenu : MonoBehaviour
{
    public UIManager _UIManager;
    // Start is called before the first frame update
    void Awake()
    {
        _UIManager = FindObjectOfType<UIManager>();
        if (_UIManager == null)
        {
            Debug.LogError("UIManager not found in scene");
        }
        else
        {
            _UIManager.ShowMenu();
        }
    }
}
