using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class SphereAppearanceLoader : MonoBehaviour
{
    public TextMeshProUGUI usernameText; // For displaying the name
    private string jsonFilePath;

    void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "player_skin.json");

        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            PlayerSkinData skinData = JsonUtility.FromJson<PlayerSkinData>(json);

            // Set sphere color
            if (colorMapping.TryGetValue(skinData.sphereColor, out Color color))
            {
                GetComponent<Renderer>().material.color = color;
            }

            // Add hat if required
            Transform hat = transform.Find("Hat");
            if (hat != null)
            {
                hat.gameObject.SetActive(skinData.hasHat);
            }

            // Set username text if available
            if (usernameText != null)
            {
                usernameText.text = skinData.name;
            }
        }
    }

    private Dictionary<string, Color> colorMapping = new Dictionary<string, Color>
    {
        { "white", Color.white },
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
}
