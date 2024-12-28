using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneManagerVR : MonoBehaviour
{
    public GameObject[] interactableObjects;
    public Canvas uiCanvas;
    public Button confirmationButton;
    public TextMeshProUGUI sceneNameText;

    private GameObject selectedObject;

    void Start()
    {
        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UI Canvas is not assigned in the SceneManagerVR!");
        }

        foreach (GameObject obj in interactableObjects)
        {
            XRGrabInteractable grabInteractable = obj.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnObjectGrabbed);
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} does not have an XRGrabInteractable component!");
            }
        }

        if (confirmationButton != null)
        {
            confirmationButton.onClick.AddListener(OnConfirmationButtonPressed);
        }
        else
        {
            Debug.LogWarning("Confirmation button is not assigned in the SceneManagerVR!");
        }
    }

    private void OnObjectGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;

        if (grabbedObject != selectedObject)
        {
            Debug.Log($"Object {grabbedObject.name} is now selected.");
            selectedObject = grabbedObject;

            // Activates the canvas when the first object is grabbed
            if (uiCanvas != null && !uiCanvas.gameObject.activeSelf)
            {
                uiCanvas.gameObject.SetActive(true);
            }
        }

        UpdateSceneNameText();
    }

    private void UpdateSceneNameText()
    {
        if (sceneNameText == null)
        {
            Debug.LogWarning("Scene name text UI is not assigned!");
            return;
        }

        if (selectedObject == null)
        {
            sceneNameText.text = "No Scene Selected";
            return;
        }

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (selectedObject == interactableObjects[i])
            {
                switch (i)
                {
                    case 0:
                        sceneNameText.text = "Customization Scene";
                        break;
                    case 1:
                        sceneNameText.text = "Macao Scene";
                        break;
                    case 2:
                        sceneNameText.text = "Chess Scene";
                        break;
                    default:
                        sceneNameText.text = "Unknown Scene";
                        break;
                }
                return;
            }
        }

        sceneNameText.text = "No Scene Selected";
    }

    private void OnConfirmationButtonPressed()
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("No object is currently selected!");
            return;
        }

        // Determine the selected object's index and load the corresponding scene
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (selectedObject == interactableObjects[i])
            {
                switch (i)
                {
                    case 0:
                        SceneManager.LoadScene("CustomizationScene");
                        break;
                    case 1:
                        SceneManager.LoadScene("MacaoScene");
                        break;
                    case 2:
                        SceneManager.LoadScene("ChessScene");
                        break;
                    default:
                        Debug.LogWarning("No scene assigned for this object!");
                        break;
                }
                return;
            }
        }

        Debug.LogWarning("Selected object does not match any interactable objects!");
    }

    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }

    void OnDestroy()
    {
        foreach (GameObject obj in interactableObjects)
        {
            if(obj != null)
            {
                XRGrabInteractable grabInteractable = obj.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.selectEntered.RemoveListener(OnObjectGrabbed);
                }
            }
        }
    }
}
