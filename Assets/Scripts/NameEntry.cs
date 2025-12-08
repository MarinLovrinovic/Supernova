using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button submitButton;

    public void SubmitName()
    {
        BasicSpawner.instance.SubmitName(nameInputField.text);
        canvas.SetActive(false);
    }

    public void ActivateButton()
    {
        submitButton.interactable = true;
    }
}
