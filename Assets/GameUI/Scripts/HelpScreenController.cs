using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreenController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button NextButton;
    [SerializeField] private Button PrevButton;
    [SerializeField] private Button ReturnButton;

    [Header("Controls")]
    [SerializeField] private TMP_Text GroupHeader;
    [SerializeField] private GameObject MovementGroup;
    [SerializeField] private GameObject MouseGroup;
    [SerializeField] private GameObject InventoryGroup;
    [SerializeField] private GameObject MenuGroup;
    private int currentControls = 0;

	void Start()
	{
		ShowMovementControls();
        NextButton.onClick.AddListener(OnNextButtonPressed);
        PrevButton.onClick.AddListener(OnPrevButtonPressed);
	}

    private void ShowMovementControls()
	{
		GroupHeader.text = "Movement";
        MovementGroup.SetActive(true);
        InventoryGroup.SetActive(false);
        MenuGroup.SetActive(false);
        PrevButton.gameObject.SetActive(false);
        NextButton.gameObject.SetActive(true);
        currentControls = 0;
	}

    private void ShowInventoryControls()
	{
		GroupHeader.text = "Inventory";
        MovementGroup.SetActive(false);
        InventoryGroup.SetActive(true);
        MenuGroup.SetActive(false);
        PrevButton.gameObject.SetActive(true);
        NextButton.gameObject.SetActive(true);
        currentControls = 1;
	}

    private void ShowMenuControls()
	{
		GroupHeader.text = "Menu";
        MovementGroup.SetActive(false);
        InventoryGroup.SetActive(false);
        MenuGroup.SetActive(true);
        PrevButton.gameObject.SetActive(true);
        NextButton.gameObject.SetActive(false);
        currentControls = 2;
	}

    public void OnNextButtonPressed()
	{
		if (currentControls == 0)
		{
			ShowInventoryControls();
		}
		else if (currentControls == 1)
		{
			ShowMenuControls();
		}
	}

    public void OnPrevButtonPressed()
	{
		if (currentControls == 1)
		{
			ShowMovementControls();
		}
		else if (currentControls == 2)
		{
			ShowInventoryControls();
		}
	}

    public void SetReturnButtonOnPress(Action action)
	{
		ReturnButton.onClick.AddListener(new(action));
	}
}
