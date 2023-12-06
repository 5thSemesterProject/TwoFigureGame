using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    #region Startup
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    private IEnumerator Start()
    {
        yield return null;
    }

    public static void EndGame()
    {
        instance.StartCoroutine(_EndGame());
    }

    private static IEnumerator _EndGame()
    {
        yield return null;
    }

    public static void SoftPauseGame()
    {

    }
}
