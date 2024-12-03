using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class Chessboard : MonoBehaviour
{
    [Header("Art stuff here")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 0.45f;
    [SerializeField] private float yOffset = 0.012f;
    [SerializeField] private Vector3 boardCenter = new(0.0f, 0.01f, -3.93f);

    [Header("Prefabs && Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private XRDirectInteractor whiteInteractor;    // right hand of player White
    // [SerializeField] private XRDirectInteractor blackInteractor; // right hand of player Black (if 2 players)

    // LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool currentPlayerWhite = true;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

    private void OnEnable()
    {
        whiteInteractor.selectEntered.AddListener(TakeInput);
        whiteInteractor.selectExited.AddListener(StopInput);
    }

    private void OnDisable()
    {
        whiteInteractor.selectEntered.RemoveListener(TakeInput);
        whiteInteractor.selectExited.RemoveListener(StopInput);
    }

    private void TakeInput(SelectEnterEventArgs args)
    {
        // Get the GameObject associated with the selected interactable
        GameObject selectedObject = args.interactableObject.transform.gameObject;

        // Iterate through all chess pieces to find the selected one
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null && chessPieces[x, y].gameObject == selectedObject)
                {
                    // Store the currently selected piece
                    currentlyDragging = chessPieces[x, y];

                    Debug.Log($"Selected Chess Piece: {currentlyDragging.type} at {x}, {y}");
                    return;
                }
            }
        }

        Debug.LogWarning("Selected object is not a chess piece.");

    }

    private void StopInput(SelectExitEventArgs args)
    {
        if (currentlyDragging == null)
        {
            Debug.LogWarning("No piece is currently being dragged.");
            return;
        }

        // Get the position where the piece was dropped
        Vector3 droppedPosition = currentlyDragging.transform.position;

        // Calculate the closest tile indices
        int closestX = Mathf.FloorToInt((droppedPosition.x + bounds.x) / tileSize);
        int closestY = Mathf.FloorToInt((droppedPosition.z + bounds.z) / tileSize);

        // Clamp the values to ensure they're within the board bounds
        closestX = Mathf.Clamp(closestX, 0, TILE_COUNT_X - 1);
        closestY = Mathf.Clamp(closestY, 0, TILE_COUNT_Y - 1);


        //tiles[closestX, closestY].GetComponent<Renderer>().material.color = Color.green;

        Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

        // Attempt to move the piece
        bool validMove = MoveTo(currentlyDragging, closestX, closestY);

        if (validMove)
        {
            Debug.Log($"Piece moved to {closestX}, {closestY}");
        }
        else
        {
            // If the move is invalid, snap the piece back to its original position
            PositionSinglePiece(currentlyDragging.currentX, currentlyDragging.currentY);
        }

        // Reset currentlyDragging to null after the move
        currentlyDragging = null;

    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            // Get the indexes of the tile i have hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);
            //// If we're hovering a tile after not hovering any tiles
            //if ( currentHover == -Vector2Int.one)
            //{
            //    currentHover = hitPosition;
            //    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            //}
            //// If we are already hovering a tile, change the previous one
            //if (currentHover != hitPosition)
            //{
            //    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
            //    currentHover = hitPosition;
            //    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            //}
        } else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }

        // For 2 player mode: makes sure that only the current player can move pieces with right hand.
        whiteInteractor.allowSelect = currentPlayerWhite;
        //blackInteractor.allowSelect = !currentPlayerWhite;    // uncomment if 2 player mode is added


        
    }
    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;


        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x,y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tirs = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tirs;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawn the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece piece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        piece.type = type;
        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return piece;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        Debug.Log("PositionSinglePiece called for x = " + x + ", y = " + y);
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetTileCenter(x, y);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    // Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; // Invalid
    }

    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        // Check if the target tile is occupied
        if (chessPieces[x, y] != null)
        {
            Debug.LogWarning($"Cannot move to ({x}, {y}): Tile is already occupied.");
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        return true;
    }
}
