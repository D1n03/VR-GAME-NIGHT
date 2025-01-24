using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HoldCards : MonoBehaviour
{
    private readonly List<GameObject> pivotPoints = new();
    public float rotationSpan = 45.0f;
    public float cardDepth = 0.0005f;
    private int? selectedCardIndex = null;
    public GameObject playedCardsObject;
    public GameObject spawnCardsObject;
    public GameObject selectPoint;
    public GameObject macaoButton;
    private PlayedCards playedCards;
    private SpawnCards spawnCards;
    public float selectCardMaxDistance = 0.1f;
    private Vector3 cardOffset = new(0.0f, -0.2f, 0.0f);
    private InputDevice rightController;
    public int skipTurnCount = 0;
    private TextMeshPro textMeshPro;
    private long? jinxStartTime = null;
    private readonly long jinxDuration = 10_000;
    public bool CanBeJinxed => jinxStartTime.HasValue;

    void Start()
    {
        playedCards = playedCardsObject.GetComponent<PlayedCards>();
        spawnCards = spawnCardsObject.GetComponent<SpawnCards>();
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        textMeshPro = transform.GetChild(0).GetComponent<TextMeshPro>();
        macaoButton.SetActive(false);

        var xrSimpleInteractable = macaoButton.GetComponentInChildren<XRSimpleInteractable>();
        xrSimpleInteractable.hoverEntered.AddListener((HoverEnterEventArgs args) =>
        {
            jinxStartTime = null;
            macaoButton.SetActive(false);
        });
    }

    void Update()
    {
        for (int i = 0; i < pivotPoints.Count; i++)
        {
            var pivotPoint = pivotPoints[i];
            var intervalCount = pivotPoints.Count + 1;

            //set pivot rotation and position
            pivotPoint.transform.localEulerAngles = new Vector3(
                0.0f,
                180.0f,
                -((i + 1) * (rotationSpan / intervalCount) - rotationSpan * 0.5f)
            );

            pivotPoint.transform.localPosition = new Vector3(
                0.0f,
                0.0f,
                (i - pivotPoints.Count * 0.5f) * cardDepth
            );

            // set card position
            var card = pivotPoint.transform.GetChild(0);
            var position = cardOffset;
            if (i == selectedCardIndex)
                position.y += -0.03f;
            card.transform.localPosition = position;
        }

        FindSelectedCard();

        if ((rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed) && isPressed) || Input.GetKeyDown(KeyCode.B))
        {
            PlaceCardDown();
        }

        textMeshPro.text = skipTurnCount == 0 ? "" : $"Wait {skipTurnCount} turn{(skipTurnCount == 1 ? "" : "s")}";

        if (pivotPoints.Count == 0)
        {
            textMeshPro.text = "You won!";
        }

        if (jinxStartTime.HasValue)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now - jinxStartTime.Value >= jinxDuration)
            {
                JinxSelf();
            }
        }
    }

    public void JinxSelf()
    {
        var extractedCards = spawnCards.ExtractCards(5);
        InsertCardsInHand(extractedCards);
        jinxStartTime = null;
        macaoButton.SetActive(false);
    }

    private void FindSelectedCard()
    {
        // find nearest card to tip of the fingers
        selectedCardIndex = null;
        var minDist = 0.0f;
        for (int i = 0; i < pivotPoints.Count; i++)
        {
            var dist = Vector3.Distance(pivotPoints[i].transform.GetChild(1).position, selectPoint.transform.position);
            if (dist < selectCardMaxDistance)
            {
                if (selectedCardIndex == null || dist < minDist)
                {
                    selectedCardIndex = i;
                    minDist = dist;
                }
            }
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
        var cardCenter = new GameObject("Card Center");

        //create parent structure self -> pivot point -> card, card center
        pivotPoint.transform.SetParent(transform, false);
        card.transform.SetParent(pivotPoint.transform, false);
        cardCenter.transform.SetParent(pivotPoint.transform, false);

        //set positions for pivot point, card and card center
        pivotPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        card.transform.localPosition = cardOffset;
        cardCenter.transform.localPosition = cardOffset;

        pivotPoints.Add(pivotPoint);
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

        int index = selectedCardIndex.Value;

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
        var card = GetSelectedCard();
        if (card == null)
        {
            return;
        }
        if (playedCards.CanPlaceCard(card) && skipTurnCount == 0)
        {
            if (CanBeJinxed)
            {
                JinxSelf();
                return;
            }
            ExtractSelectedCard();
            playedCards.PlaceCardOnTop(card);

            if (pivotPoints.Count == 1)
            {
                jinxStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                macaoButton.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Invalid card");
        }
    }
}
