using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayedCards : MonoBehaviour
{
    private readonly List<GameObject> cards = new();
    public GameObject spawnCardsObject;
    private SpawnCards spawnCards;
    // Start is called before the first frame update
    void Start()
    {
        spawnCards = spawnCardsObject.GetComponent<SpawnCards>();
        var card = spawnCards.ExtractCard();
        if (card != null )
        {
            cards.Add( card );
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.transform.localPosition = i * spawnCards.CardInterval * spawnCards.up;
        }
    }

    public void PlaceCardOnTop(GameObject card)
    {
        card.transform.SetParent(transform, false);
        card.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
        cards.Add(card);
    }

    public bool CanPlaceCard(GameObject card)
    {
        if (cards.Count > 0)
        {
            var topCard = cards[^1];
            ExtractNumberAndTypeFromCard(card, out int cardNumber, out string cardType);
            ExtractNumberAndTypeFromCard(topCard, out int topCardNumber, out string topCardType);
            return cardType == topCardType || cardNumber == topCardNumber;
        }
        else
        {
            return true;
        }
    }

    private void ExtractNumberAndTypeFromCard(GameObject card, out int cardNumber, out string cardType)
    {
        var cardVariables = card.GetComponent<Variables>();
        cardType = cardVariables.declarations.Get<string>("type");
        cardNumber = cardVariables.declarations.Get<int>("number");
    }
}
