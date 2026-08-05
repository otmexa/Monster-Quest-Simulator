using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSelectionUI : MonoBehaviour
{
    public GameObject selectionPanel;
    public Button btnSword;
    public Button btnAxe; 
    public Button btnConfirm;
    public PlayerCombat playerCombat;

    private int selectedWeapon = -1; // -1 means no selection
    
    private Color defaultColor = Color.white;
    private Color selectedColor = new Color(0.2f, 0.6f, 1.0f); // Blue
    private Color confirmDisabledColor = Color.gray;
    private Color confirmEnabledColor = Color.green;

    void Start()
    {
        Time.timeScale = 0f;
        selectionPanel.SetActive(true);

        // Set initial confirm button state
        btnConfirm.interactable = false;
        btnConfirm.GetComponent<Image>().color = confirmDisabledColor;

        btnSword.onClick.AddListener(() => SelectWeapon(0, btnSword));
        btnAxe.onClick.AddListener(() => SelectWeapon(1, btnAxe));
        btnConfirm.onClick.AddListener(ConfirmSelection);
    }

    void SelectWeapon(int weaponIndex, Button clickedBtn)
    {
        selectedWeapon = weaponIndex;
        Debug.Log("Selected weapon: " + weaponIndex);
        
        // Reset colors
        btnSword.GetComponent<Image>().color = defaultColor;
        btnAxe.GetComponent<Image>().color = defaultColor;
        
        // Highlight selected
        clickedBtn.GetComponent<Image>().color = selectedColor;
        
        // Enable confirm button
        btnConfirm.interactable = true;
        btnConfirm.GetComponent<Image>().color = confirmEnabledColor;
    }

    void ConfirmSelection()
    {
        if (selectedWeapon == -1) return; // Should not happen since button is disabled, but just in case
        
        Debug.Log("Weapon Confirmed!");
        selectionPanel.SetActive(false);
        Time.timeScale = 1f;

        if (playerCombat != null && playerCombat.swordPlaceholder != null)
        {
            playerCombat.swordPlaceholder.SetActive(true);
        }
    }
}
