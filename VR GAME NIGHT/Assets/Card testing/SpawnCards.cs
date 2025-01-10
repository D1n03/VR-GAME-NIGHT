using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnCards : MonoBehaviour
{
    public GameObject[] prefabToSpawn;
    public Transform parentObject;
    public GameObject holdPoint;
    private HoldCards holdCards;
    private readonly List<GameObject> cards = new();
    public float cardStackHeight = 0.05f;
    private int CardCount => prefabToSpawn.Length;
    public Vector3 up = new(0.0f, 0.0f, 1.0f);
    public float CardInterval => cardStackHeight / CardCount;

    void Spawn()
    {
        holdCards = holdPoint.GetComponent<HoldCards>();
        
        var cardPermutation = GetPermutation(CardCount);
        if (prefabToSpawn != null && parentObject != null)
        {
            for (int i = 0; i<CardCount; i++)
            {
                GameObject spawnedCard = prefabToSpawn[cardPermutation[i]];
                spawnedCard.transform.SetParent(parentObject, false);

                var collider = spawnedCard.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.isTrigger = true;


                spawnedCard.transform.localPosition = CardInterval * i * up;
                spawnedCard.transform.localEulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
                //spawnedObject.transform.localRotation = Quaternion.identity;

                var name = spawnedCard.name;
                var variables = spawnedCard.AddComponent<Variables>();
                var cardType = name[..^2];
                var cardNumber = int.Parse(name.Substring(name.Length - 2, 2));
                variables.declarations.Set("type", cardType);
                variables.declarations.Set("number", cardNumber);

                cards.Add(spawnedCard);
            }
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
    void Awake()
    {
        Spawn();


        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(TakeCard);

    }

    void TakeCard(SelectEnterEventArgs arg)
    {
        GameObject card = ExtractCard();
        if (card == null)
        {
            return;
        }

        holdCards.InsertCardInHand(card);
    }

    public GameObject ExtractCard()
    {
        var lastIndex = cards.Count - 1;
        if (lastIndex < 0)
        {
            return null;
        }
        var card = cards[lastIndex];
        cards.RemoveAt(lastIndex);
        return card;
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
