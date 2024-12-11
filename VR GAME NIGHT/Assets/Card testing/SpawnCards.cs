using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnCards : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform parentObject;
    public GameObject holdPoint;
    private readonly List<GameObject> cards = new();
    private XRSimpleInteractable interactable;
    public readonly float cardStackHeight = 0.05f;
    private readonly int cardCount = 26;
    private Vector3 up = new(0.0f, 0.0f, 1.0f);

    private float CardInterval => cardStackHeight / cardCount;

    void Spawn()
    {
        var cardPermutation = GetPermutation(cardCount);
        if (prefabToSpawn != null && parentObject != null)
        {
            for (int i = 0; i<cardCount; i++)
            {
                // Instantiate the prefab as a child of the parentObject
                GameObject spawnedObject = Instantiate(prefabToSpawn, parentObject);
                GameObject textchild = spawnedObject.GetNamedChild("Text");
                textchild.GetComponent<TextMeshPro>().text = ((char)('A' + cardPermutation[i])).ToString();

                // Optional: Reset the local position and rotation of the spawned object
                spawnedObject.transform.localPosition = CardInterval * i * up;
                //spawnedObject.transform.localRotation = Quaternion.identity;
                cards.Add(spawnedObject);
            }

            Debug.Log("Prefab spawned as a child of the parent object.");
        }
        else
        {
            Debug.LogError("Prefab or parent object is not assigned!");
        }
    }

    public static List<int> GetPermutation(int n)
    {
        // Create a list of numbers from 0 to n-1
        List<int> numbers = new();
        for (int i = 0; i < n; i++)
        {
            numbers.Add(i);
        }

        // Shuffle the list to create a permutation
        System.Random random = new();
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            // Swap the current element with a random element before it
            int j = random.Next(i + 1);
            (numbers[j], numbers[i]) = (numbers[i], numbers[j]);
        }

        // Return the shuffled list
        return numbers;
    }

    // Start is called before the first frame update
    void Start()
    {
        Spawn();

        // Get the XR Simple Interactable component
        
        if (TryGetComponent<XRSimpleInteractable>(out interactable))
        {
            // Register event listeners for select entered and exited
            interactable.selectEntered.AddListener(TakeCard);
            //interactable.onSelectExited.AddListener(OnSelectExited);
            Debug.Log("added listener");
        }
        else
        {
            Debug.LogError("XR Simple Interactable component not found on " + gameObject.name);
        }
    }

    void TakeCard(SelectEnterEventArgs arg)
    {
        var lastIndex = cards.Count - 1;
        if (lastIndex < 0)
        {
            return;
        }
        var card = cards[lastIndex];
        cards.RemoveAt(lastIndex);

        var pivotPoint = new GameObject("Pivot Point");

        pivotPoint.transform.SetParent(holdPoint.transform, false);
        card.transform.SetParent(pivotPoint.transform, false);

        var offset = new Vector3(0.0f, -0.2f, 0.0f);

        pivotPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        card.transform.localPosition = offset;

        var holdCards = holdPoint.GetComponent<HoldCards>();
        holdCards.pivotPoints.Add(pivotPoint);

        Debug.Log("Took a card");
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.transform.localPosition = i * CardInterval * up;
        }
    }
}
