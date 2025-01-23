using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayedCards : MonoBehaviour
{
    private readonly List<GameObject> cards = new();
    public GameObject spawnCardsObject;
    private SpawnCards spawnCards;

    void Start()
    {
        spawnCards = spawnCardsObject.GetComponent<SpawnCards>();
    }

    void Update()
    {
        // set the positions of the cards in the stack
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.transform.localPosition = i * spawnCards.CardInterval * spawnCards.up;
            //print(card.transform.localPosition + " " + i);
        }
    }

    public void PlaceCardOnTop(GameObject card)
    {
        card.transform.SetParent(transform, false);
        var maxDeviation = 30.0f;
        var randomAngle = Random.Range(-maxDeviation, maxDeviation);
        card.transform.localEulerAngles = new Vector3(0.0f, randomAngle, 0.0f);
        cards.Add(card);

        ExtractNumberAndTypeFromCard(card, out int cardNumber, out _);
        switch (cardNumber)
        {
            case 2: case 3:
                spawnCards.takeCardCount += cardNumber;
                break;
            case 4:
                spawnCards.takeCardCount = 0;
                break;
        }
    }

    public bool CanPlaceCard(GameObject card)
    {
        if (cards.Count == 0)
        {
            return true;
        }
  
        var topCard = cards[^1];
        ExtractNumberAndTypeFromCard(card, out int cardNumber, out string cardType);
        ExtractNumberAndTypeFromCard(topCard, out int topCardNumber, out string topCardType);

        if (spawnCards.takeCardCount > 0)
        {
            return cardNumber == 2 || cardNumber == 3 || cardNumber == 4;
        }
        else
        {
            return cardType == topCardType || cardNumber == topCardNumber;
        }
        
    }

    private void ExtractNumberAndTypeFromCard(GameObject card, out int cardNumber, out string cardType)
    {
        var cardVariables = card.GetComponent<Variables>();
        cardType = cardVariables.declarations.Get<string>("type");
        cardNumber = cardVariables.declarations.Get<int>("number");
    }

    public List<GameObject> ExtractUnusedCards()
    {
        if (cards.Count < 2)
        {
            return new();
        }

        var extractCount = cards.Count - 1;
        var unusedCards = cards.Take(extractCount).ToList();
        cards.RemoveRange(0, extractCount);

        return unusedCards;
    }
}
