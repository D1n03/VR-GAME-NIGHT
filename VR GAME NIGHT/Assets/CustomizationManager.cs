using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;
using Keyboard;
using UnityEngine.SceneManagement;

public class CustomizationManager : MonoBehaviour
{
    public GameObject customizableSphere;
    public GameObject hatObject;
    public GameObject facePlane;
    public TextMeshProUGUI usernameText;
    public Button whiteButton, purpleButton, blueButton, indigoButton, redButton, orangeButton, yellowButton, greenButton;
    public Toggle topHatToggle;
    public Button nextFaceButton, previousFaceButton;

    [Tooltip("Assign face textures in order")]
    public Texture[] faceTextures; // Array of textures to be assigned in the Inspector

    private string jsonFilePath;
    private PlayerSkinData skinData;

    [SerializeField] private KeyboardManager keyboardManager;

    private void Start()
    {
        if (keyboardManager != null)
        {
            keyboardManager.onEnterPressed.AddListener(UpdatePlayerName);
        }

        jsonFilePath = Path.Combine(Application.persistentDataPath, "player_skin.json");

        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            skinData = JsonUtility.FromJson<PlayerSkinData>(json);
        }
        else
        {
            skinData = new PlayerSkinData();
            SaveSkinData();
        }

        // Set initial appearance
        ApplySkinData();

        whiteButton.onClick.AddListener(() => UpdateSphereColor("white"));
        purpleButton.onClick.AddListener(() => UpdateSphereColor("purple"));
        blueButton.onClick.AddListener(() => UpdateSphereColor("blue"));
        indigoButton.onClick.AddListener(() => UpdateSphereColor("indigo"));
        redButton.onClick.AddListener(() => UpdateSphereColor("red"));
        orangeButton.onClick.AddListener(() => UpdateSphereColor("orange"));
        yellowButton.onClick.AddListener(() => UpdateSphereColor("yellow"));
        greenButton.onClick.AddListener(() => UpdateSphereColor("green"));
        topHatToggle.onValueChanged.AddListener(UpdateHatStatus);


        nextFaceButton.onClick.AddListener(NextFace);
        previousFaceButton.onClick.AddListener(PreviousFace);

    }

    private void UpdateSphereColor(string color)
    {
        skinData.sphereColor = color;
        SaveSkinData();
        ApplySkinData();
    }

    private void UpdateHatStatus(bool isOn)
    {
        skinData.hasHat = isOn;
        SaveSkinData();
        ApplySkinData();
    }

    public void UpdatePlayerName(string newName)
    {
        skinData.name = newName;
        SaveSkinData();
        ApplySkinData();
    }

    private void ApplySkinData()
    {
        if (colorMapping.TryGetValue(skinData.sphereColor, out Color color))
        {
            customizableSphere.GetComponent<Renderer>().material.color = color;
        }

        UpdateFaceTexture();

        if (hatObject != null)
        {
            hatObject.SetActive(skinData.hasHat);
        }

        // Update toggle state to match JSON data
        topHatToggle.SetIsOnWithoutNotify(skinData.hasHat);

        if (usernameText != null)
        {
            usernameText.text = skinData.name;
        }
    }

    private void SaveSkinData()
    {
        string json = JsonUtility.ToJson(skinData, true);
        File.WriteAllText(jsonFilePath, json);
    }

    private void UpdateFaceTexture()
    {
        if (faceTextures == null || faceTextures.Length == 0)
        {
            Debug.LogWarning("Face textures are not assigned!");
            return;
        }

        if (skinData.faceType >= 0 && skinData.faceType < faceTextures.Length)
        {
            Renderer renderer = facePlane.GetComponent<Renderer>();
            Material material = renderer.material;
            material.SetTexture("_MainTex", faceTextures[skinData.faceType]);
        }
        else
        {
            Debug.LogWarning($"Face type index {skinData.faceType} is out of bounds!");
        }
    }

    private void NextFace()
    {
        print("next face");
        skinData.faceType = (skinData.faceType + 1) % 15; // Assuming 15 face textures (0 to 14)
        SaveSkinData();
        ApplySkinData();
    }

    private void PreviousFace()
    {
        print("old face");
        skinData.faceType = (skinData.faceType - 1 + 15) % 15; // Wrap around to 14 if it goes below 0
        SaveSkinData();
        ApplySkinData();
    }


    private Dictionary<string, Color> colorMapping = new Dictionary<string, Color>
    {
        { "white", Color.white },
        { "purple", ParseColor("#972EC5") },
        { "blue", ParseColor("#00EEFF") },
        { "indigo", ParseColor("#0E1BD9") },
        { "red", ParseColor("#FF1100") },
        { "orange", ParseColor("#FF7700") },
        { "yellow", ParseColor("#F9EA21") },
        { "green", ParseColor("#6CC329") },

    };

    // Helper method to parse hex color strings
    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        else
        {
            Debug.LogError($"Invalid color code: {hex}");
            return Color.white; // Default fallback color
        }
    }

    public void OnExitButton()
    {
        SceneManager.LoadScene("StartScene");
    }
}
