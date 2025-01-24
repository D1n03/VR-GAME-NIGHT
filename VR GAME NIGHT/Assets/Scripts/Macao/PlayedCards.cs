using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayedCards : MonoBehaviour
{
    private readonly List<GameObject> cards = new();
    public GameObject spawnCardsObject;
    private SpawnCards spawnCards;
    private string wildCardType = null;
    public bool choosingWildCardType = false;
    public GameObject suits;

    void Start()
    {
        spawnCards = spawnCardsObject.GetComponent<SpawnCards>();

        suits.SetActive(false);
        foreach (var xrSimpleInteractable in suits.transform.GetComponentsInChildren<XRSimpleInteractable>())
        {
            xrSimpleInteractable.hoverEntered.AddListener((HoverEnterEventArgs args) =>
            {
                wildCardType = xrSimpleInteractable.gameObject.name;
                choosingWildCardType = false;
                suits.SetActive(false);
            });
        }
    }

    void Update()
    {
        // set the positions of the cards in the stack
        var up = new Vector3(0.0f, 1.0f, 0.0f);
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.transform.localPosition = i * spawnCards.CardInterval * up;
        }
    }

    public void PlaceCardOnTop(GameObject card)
    {
        card.transform.SetParent(transform, false);
        var maxDeviation = 30.0f;
        var randomAngle = Random.Range(-maxDeviation, maxDeviation);
        card.transform.localEulerAngles = new Vector3(0.0f, randomAngle, 0.0f);
        cards.Add(card);

        wildCardType = null;

        ExtractNumberAndTypeFromCard(card, out int cardNumber, out _);
        switch (cardNumber)
        {
            case 1:
                spawnCards.skipTurnCount += 1;
                break;
            case 2:
            case 3:
                spawnCards.takeCardCount += cardNumber;
                break;
            case 4:
                spawnCards.takeCardCount = 0;
                spawnCards.skipTurnCount = 0;
                break;
            case 7:
                choosingWildCardType = true;
                suits.SetActive(true);
                break;
        }
    }

    public bool CanPlaceCard(GameObject card)
    {
        if (cards.Count == 0)
        {
            return true;
        }

        if (choosingWildCardType)
        {
            return false;
        }

        var topCard = cards[^1];
        ExtractNumberAndTypeFromCard(card, out int cardNumber, out string cardType);
        ExtractNumberAndTypeFromCard(topCard, out int topCardNumber, out string topCardType);

        if (wildCardType != null)
        {
            topCardType = wildCardType;
        }

        if (spawnCards.takeCardCount > 0)
        {
            return cardNumber == 2 || cardNumber == 3 || cardNumber == 4;
        }
        else if (spawnCards.skipTurnCount > 0)
        {
            return cardNumber == 1 || cardNumber == 4;
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
