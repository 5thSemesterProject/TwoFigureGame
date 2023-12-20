using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomFadeLogic : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Material[] materials;
    [SerializeField] private int playerLayer = 9;
    [SerializeField] private float maxFadeRadius = 50;
    [SerializeField] private float fadeTime = 1;
    [SerializeField] private float CharacterRadius = 5;

    [Header("Activation")]
    [SerializeField] private GameObject rootObject;

    private Coroutine fadeRoutine;
    private BoxCollider[] roomColliders;
    private List<GameObject> charactersInRoom = new List<GameObject>();

    //Shader Variable Names
    private string nameRadius = "_Radius";
    private string nameShouldFade = "_ShouldFade";
    private string nameEpicenter = "_Epicenter";
    private string nameInactiveChar = "_InactiveCharacter";
    private string nameCharRadius = "_CharacterRadius";

    #region Enter / Exit
    void Start()
    {
        if (rootObject == null)
        {
            Debug.LogWarning("No Room root object assigned! Room will not be toggled!");
        }

        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning("Something went wrong. No material assigned.");
        }
        else
        {
            SetVisible(false);
        }

        roomColliders = GetComponents<BoxCollider>();
        charactersInRoom.AddRange(GetCharactersInColliders());

        CustomEvents.characterSwitch -= ReevaluateActiveCharacter;
        CustomEvents.characterSwitch += ReevaluateActiveCharacter;
    }

    private void OnDisable()
    {
        SetVisible(true);
        SetMaterialVector(nameEpicenter, Vector2.zero);
        SetMaterialVector(nameInactiveChar, Vector2.zero);
        SetMaterialFloat(nameCharRadius, 0);
    }

    private void SetVisible(bool visible)
    {
        SetMaterialInt(nameShouldFade, visible ? 0 : 1);
        SetMaterialFloat(nameRadius, visible ? maxFadeRadius : 0);
        SetRoomActive(visible);
    }

    private void SetRoomActive(bool visible)
    {
        if (rootObject != null)
        {
            rootObject.SetActive(visible);
        }
    }
    #endregion

    #region Triggers
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            if (!charactersInRoom.Contains(other.gameObject))
            {
                charactersInRoom.Add(other.gameObject);
            }
            SetMaterialVector(nameEpicenter, VectorHelper.Convert3To2(other.transform.position));
            StartFade(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            GameObject[] characterInColliders = GetCharactersInColliders();
            SetMaterialFloat(nameCharRadius, 0);
            charactersInRoom.Remove(other.gameObject);

            if (characterInColliders.Length > 0)
            {
                if (characterInColliders.Length > 1|| CharacterManager.ActiveCharacterRigidbody == characterInColliders[0])
                {
                    charactersInRoom.Add(other.gameObject);
                    return;
                }

                SetMaterialVector(nameInactiveChar, VectorHelper.Convert3To2(charactersInRoom[0].gameObject.transform.position));
                SetMaterialFloat(nameCharRadius, CharacterRadius);
            }

            SetMaterialVector(nameEpicenter, VectorHelper.Convert3To2(other.transform.position));
            StartFade(false);
        }
    }

    private GameObject[] GetCharactersInColliders()
    {
        List<GameObject> charactersInColliders = new List<GameObject>();

        for (int i = 0; i < roomColliders.Length; i++)
        {
            Collider[] characterInColliders = Physics.OverlapBox(roomColliders[i].center + transform.position, roomColliders[i].size / 2, roomColliders[i].transform.rotation, LayerMask.GetMask("Player"));

            for (int j = 0; j < characterInColliders.Length; j++)
            {
                charactersInColliders.Add(characterInColliders[j].gameObject);
            }
        }

        return charactersInColliders.ToArray();
    }

    public void ReevaluateActiveCharacter(GameObject ActiveCharacter)
    {
        if (charactersInRoom.Contains(ActiveCharacter))
        {
            SetMaterialVector(nameEpicenter, VectorHelper.Convert3To2(ActiveCharacter.transform.position));
            StartFade(true);
        }
        else
        {
            if (charactersInRoom.Count > 0)
            {
                SetMaterialVector(nameInactiveChar, VectorHelper.Convert3To2(charactersInRoom[0].gameObject.transform.position));
                SetMaterialFloat(nameCharRadius, CharacterRadius);
            }
            StartFade(false);
        }
    }
    #endregion

    #region Fade Logic
    private void StartFade(bool bFadeIn)
    {
        //Stop Prior Fade
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        //Start Fade
        fadeRoutine = StartCoroutine(Fade(bFadeIn));
    }

    private IEnumerator Fade(bool bFadeIn)
    {
        //Get - Set Targets
        float currentRadius = materials[0].GetFloat(nameRadius);
        float targetRadius = bFadeIn ? maxFadeRadius : 0;
        SetMaterialFloat(nameRadius, currentRadius);

        //Make Fadable
        SetMaterialInt(nameShouldFade, 1);
        SetRoomActive(true);

        //Set Radius Over Time
        while (bFadeIn ? currentRadius < targetRadius : targetRadius < currentRadius)
        {
            float addedDeltaTime = bFadeIn ? Time.deltaTime : -Time.deltaTime;
            currentRadius += addedDeltaTime * maxFadeRadius / fadeTime;
            SetMaterialFloat(nameRadius, currentRadius);
            yield return null;
        }

        //Turn Off Fade if is visible
        SetVisible(bFadeIn);

        if (bFadeIn)
        {
            SetMaterialFloat(nameCharRadius, 0);
        }

        fadeRoutine = null;
    }
    #endregion

    #region Set Material Values
    private void SetMaterialInt(string intName, int value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetInt(intName, value);
        }
    }
    private void SetMaterialFloat(string floatName, float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(floatName, value);
        }
    }
    private void SetMaterialVector(string vectorName, Vector4 value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetVector(vectorName, value);
        }
    }
    #endregion
}
