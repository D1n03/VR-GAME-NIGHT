using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoldCards : MonoBehaviour
{
    private readonly List<GameObject> pivotPoints = new();
    public float rotationSpan = 45.0f;
    public float cardDepth = 0.0005f;
    private int? selectedCardIndex = 0;//null;
    public GameObject playedCardsPile;
    private PlayedCards playedCards;
    void Start()
    {
        playedCards = playedCardsPile.GetComponent<PlayedCards>();
        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener((SelectEnterEventArgs args) => PlaceCardDown() );
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < pivotPoints.Count; i++)
        {
            var intervalCount = pivotPoints.Count + 1;
            var pivotPoint = pivotPoints[i];
            pivotPoint.transform.localEulerAngles = new Vector3(0.0f, 180.0f, - ((i + 1) * (rotationSpan / intervalCount) - rotationSpan * 0.5f));
            var position = pivotPoint.transform.localPosition;
            var variables = pivotPoint.GetComponent<Variables>();
            variables.declarations.Set("index", i);
            position.x = 0.0f;
            position.y = i == selectedCardIndex ? - 0.03f : 0.0f;
            position.z = (i - pivotPoints.Count * 0.5f) * cardDepth;
            pivotPoint.transform.localPosition = position;
        }
    }

    public void InsertCardInHand(GameObject card)
    {
        var pivotPoint = new GameObject("Pivot Point");

        pivotPoint.transform.SetParent(transform, false);
        card.transform.SetParent(pivotPoint.transform, false);

        var offset = new Vector3(0.0f, -0.2f, 0.0f);

        pivotPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        card.transform.localPosition = offset;

        /*
        var xrInteractable = pivotPoint.AddComponent<XRSimpleInteractable>();
        xrInteractable.hoverEntered.AddListener((HoverEnterEventArgs args) => HoverCard(pivotPoint));
        xrInteractable.hoverExited.AddListener((HoverExitEventArgs args) => HoverCard(pivotPoint));
        
        var rigidBody = pivotPoint.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        */


        var variables = pivotPoint.AddComponent<Variables>();

        pivotPoints.Add(pivotPoint);

        /*
        var cardVariables = card.GetComponent<Variables>();
        var cardType = cardVariables.declarations.Get<string>("type");
        var cardNumber = cardVariables.declarations.Get<int>("number");
        Debug.Log("type: " + cardType + " number: " + cardNumber);
        */

    }

    void HoverCard(GameObject pivotPoint)
    {
        var variables = pivotPoint.GetComponent<Variables>();
        var index = variables.declarations.Get<int>("index");

        selectedCardIndex = index;
    }

    //GameObject GetCurrentCard()

    GameObject ExtractCard()
    {
        if (selectedCardIndex == null)
        {
            return null;
        }

        int index = selectedCardIndex ?? -1;

        var pivotPoint = pivotPoints[index];
        pivotPoints.RemoveAt(index);
        var card = pivotPoint.transform.GetChild(0).gameObject;
        card.transform.SetParent(transform, true);
        Destroy(pivotPoint);
        return card;
    }

    void PlaceCardDown()
    {
        Debug.Log("Trying to place card");
        var card = ExtractCard();
        if (card == null)
        {
            Debug.Log("Null card");
            return;
        }
        Debug.Log("Placed card");
        playedCards.PlaceCardOnTop(card);
    }
}
