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
    private int? selectedCardIndex = 0;//null;//0
    public GameObject playedCardsPile;
    private PlayedCards playedCards;
    void Start()
    {
        playedCards = playedCardsPile.GetComponent<PlayedCards>();
        /*
        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener((SelectEnterEventArgs args) => PlaceCardDown() );
        interactable.activated.AddListener((ActivateEventArgs args) => PlaceCardDown());
        */
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < pivotPoints.Count; i++)
        {
            var pivotPoint = pivotPoints[i];
            var intervalCount = pivotPoints.Count + 1;

            //set pivot rotation and position
            pivotPoint.transform.localEulerAngles = new Vector3(
                0.0f, 
                180.0f,
                - ((i + 1) * (rotationSpan / intervalCount) - rotationSpan * 0.5f)
            );

            pivotPoint.transform.localPosition = new Vector3(
                0.0f,
                i == selectedCardIndex ? -0.03f : 0.0f,
                (i - pivotPoints.Count * 0.5f) * cardDepth
            );

            //update index variable
            var variables = pivotPoint.GetComponent<Variables>();
            variables.declarations.Set("index", i);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaceCardDown();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedCardIndex -= 1;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedCardIndex += 1;
        }

        if (selectedCardIndex < 0)
        {
            selectedCardIndex = 0;
        }

        if (selectedCardIndex >= pivotPoints.Count)
        {
            selectedCardIndex = pivotPoints.Count - 1;
        }
    }

    public void InsertCardsInHand(List<GameObject> cards)
    {
        foreach (var card in cards)
        {
            InsertSingleCardInHand(card);
        }
    }
    private void InsertSingleCardInHand(GameObject card)
    {
        //create new pivot point
        var pivotPoint = new GameObject("Pivot Point");

        //create parent structure self -> pivot point -> card
        pivotPoint.transform.SetParent(transform, false);
        card.transform.SetParent(pivotPoint.transform, false);

        //set positions for pivot point and card
        pivotPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        card.transform.localPosition = new Vector3(0.0f, -0.2f, 0.0f);

        //highlight hovered card
        var xrInteractable = pivotPoint.AddComponent<XRSimpleInteractable>();
        xrInteractable.hoverEntered.AddListener((HoverEnterEventArgs args) => HoverCard(pivotPoint));
        xrInteractable.hoverExited.AddListener((HoverExitEventArgs args) => HoverCard(pivotPoint));

        var mesh = card.GetComponent<MeshCollider>();
        xrInteractable.colliders.Add(mesh);
        var rigidBody = pivotPoint.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        

        //add variables component for index
        pivotPoint.AddComponent<Variables>();

        pivotPoints.Add(pivotPoint);
    }

    void HoverCard(GameObject pivotPoint)
    {
        Debug.Log("called");
        var variables = pivotPoint.GetComponent<Variables>();
        var index = variables.declarations.Get<int>("index");

        selectedCardIndex = index;
    }

    GameObject GetSelectedCard()
    {
        if (selectedCardIndex == null)
        {
            return null;
        }

        int index = selectedCardIndex ?? -1;
        var pivotPoint = pivotPoints[index];
        var card = pivotPoint.transform.GetChild(0).gameObject;

        return card;
    }

    GameObject ExtractSelectedCard()
    {
        if (selectedCardIndex == null)
        {
            return null;
        }

        int index = selectedCardIndex ?? -1;

        //extract pivot point
        var pivotPoint = pivotPoints[index];
        pivotPoints.RemoveAt(index);

        //extract card
        var card = pivotPoint.transform.GetChild(0).gameObject;
        card.transform.SetParent(transform, true);

        //destroy pivot point
        Destroy(pivotPoint);

        return card;
    }

    void PlaceCardDown()
    {
        Debug.Log("Trying to place card");
        var card = GetSelectedCard();
        if (playedCards.CanPlaceCard(card))
        {
            ExtractSelectedCard();
            playedCards.PlaceCardOnTop(card);
            Debug.Log("Placed card");
        }
        else
        {
            Debug.Log("Invalid card");
        }
    }
}
