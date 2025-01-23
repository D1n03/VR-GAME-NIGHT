using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnCards : MonoBehaviour
{
    public GameObject[] cardsToSpawn;
    public GameObject holdCardsObject;
    public GameObject playedCardsObject;
    private HoldCards holdCards;
    private PlayedCards playedCards;
    private TextMeshPro textMeshPro;
    private readonly List<GameObject> cards = new();
    public float cardStackHeight = 0.05f;
    private int CardCount => cardsToSpawn.Length;
    public Vector3 up = new(0.0f, 0.0f, 1.0f);
    public float CardInterval => cardStackHeight / CardCount;
    public int takeCardCount = 0;

    void Spawn()
    {
        var cardPermutation = GetPermutation(CardCount);
        List<GameObject> spawnedCards = new();
        for (int i = 0; i < CardCount; i++)
        {
            //get random card
            GameObject spawnedCard = cardsToSpawn[cardPermutation[i]];

            //configure collider
            var collider = spawnedCard.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;
            collider.providesContacts = true;

            //extract card numbr and type in variables component
            var name = spawnedCard.name;
            var variables = spawnedCard.AddComponent<Variables>();
            var cardType = name[..^2];
            var cardNumber = int.Parse(name.Substring(name.Length - 2, 2));
            variables.declarations.Set("type", cardType);
            variables.declarations.Set("number", cardNumber);

            spawnedCards.Add(spawnedCard);
        }

        InsertCards(spawnedCards);
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

    void TakeCards(SelectEnterEventArgs arg)
    {
        var extractedCards = ExtractCards();
        holdCards.InsertCardsInHand(extractedCards);
    }

    public List<GameObject> ExtractCards(int? count = null)
    {
        int extractCount = count ?? Math.Max(takeCardCount, 1);
        if (takeCardCount > 0)
        {
            takeCardCount = 0;
        }
        if (extractCount >= cards.Count)
        {
            var unusedCards = playedCards.ExtractUnusedCards();
            unusedCards.Reverse();
            InsertCards(unusedCards);
        }

        var extractedCards = cards.TakeLast(extractCount).ToList();
        cards.RemoveRange(cards.Count - extractCount, extractCount);

        return extractedCards;
    }

    public void InsertCards(List<GameObject> cards)
    {
        foreach (var card in cards)
        {
            card.transform.SetParent(transform, false);
            card.transform.localEulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
        }
        this.cards.InsertRange(0, cards);
    }

    void Awake()
    {
        holdCards = holdCardsObject.GetComponent<HoldCards>();
        playedCards = playedCardsObject.GetComponent<PlayedCards>();
        textMeshPro = transform.GetChild(0).GetComponent<TextMeshPro>();

        Spawn();

        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(TakeCards);

    }

    private void Start()
    {
        var startingCards = ExtractCards(5);
        holdCards.InsertCardsInHand(startingCards);

        /*
        var firstCard = ExtractCards(1);
        var card = firstCard.First();
        var variables = card.GetComponent<Variables>();
        var cardNumber = variables.declarations.Get<int>("number");

        while (cardNumber == 1 || cardNumber == 2 || cardNumber == 3 || cardNumber == 4 || cardNumber == 7)
        {
            InsertCards(firstCard);
            firstCard = ExtractCards(1);
            card = firstCard.First();
            variables = card.GetComponent<Variables>();
            cardNumber = variables.declarations.Get<int>("number");
        }

        playedCards.PlaceCardOnTop(card);
        */

        
        var aLotOfCards = ExtractCards(45);
        foreach (var card in aLotOfCards)
        {
            playedCards.PlaceCardOnTop(card);
        }
        
    }

    void Update()
    {
        // set the positions of the cards in the stack
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.transform.localPosition = i * CardInterval * up;
            //print(card.transform.localPosition + " " + i);
        }

        textMeshPro.transform.localPosition = cards.Count * CardInterval * up;
        textMeshPro.text = (takeCardCount == 0 ? "" : "+" + takeCardCount);
    }
}
