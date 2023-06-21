using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    // The state machine that manages what is happening.
    // Should have a Board reference.
    public GameBoard gameBoard = new GameBoard();
    public MinoEnum[,] currentGameState = null;

    public GameState gameState = GameState.None;
    public GameState previousGameState = GameState.None;

    public Settings gameSettings = new Settings();

    public List<Piece> currentPieceBag;
    public List<Piece> nextPieceBag;

    public ActivePiece currentPiece;
    public Piece? heldPiece = null;

    public Point currentLocation;

    public object lastKeyPressed;
    public int keyPressDuration;

    public List<Tuple<Piece, MinoEnum[,], Piece?>> previousPieces = new(); // <Current piece, entire gameboard before Piece was placed, held piece>

    public UserInput userInput;

    private Size _moveLeft = new Size(-1, 0);
    private Size _moveRight = new Size(1, 0);
    private Size _moveDown = new Size(0, -1);
    private Size _moveUp = new Size(0, 1);

    private int _rotateLeft = -1;
    private int _rotateRight = 1;

    private int _searchPosition = -1;

    public int[] completedRows;
    private bool heldPieceThisMove = false;

    // Awake is basically the constructor.
    void Awake()
    {
        gameBoard.Initialize();
        gameBoard.DrawGameBoard();

        currentPieceBag = PieceBagManager.GeneratePieceBag();
        nextPieceBag = PieceBagManager.GeneratePieceBag();

        foreach (Piece piece in currentPieceBag)
        {
            Debug.Log(Tetrominos.GetPieceData(piece));
        }

        //userInput = this.AddComponent<UserInput>();
        userInput.SendSettings(gameSettings);
    }

    // Update is called once per frame
    void Update()
    {
        // If menu is pressed, then handle that before gamestate logic.
        if ((userInput.GetKeysPressed & (int)KeyPressed.Menu) == (int)KeyPressed.Menu)
        {
            // Do Menu Things.
            // If in menu, set gameState = previousGameState; previousGameState = GameState.None;
            // if not in menu, set previousGameState = gameState; gameState = GameState.Menu;
            // But, for now, just quit the game.
            Application.Quit();
        }

        switch (gameState)
        {
            case GameState.None:
                gameState = GameState.SpawnPiece;
                break;
            case GameState.SpawnPiece:
                currentGameState = gameBoard.CopyBoardState();

                GetNextPiece();
                gameBoard.DrawNextPieces(currentPieceBag, nextPieceBag);
                break;
            case GameState.MovePiece:
                RespondToGameplayInput();
                break;
            case GameState.PiecePlaced:
                previousPieces.Add(new Tuple<Piece, MinoEnum[,], Piece?>(currentPiece.Piece, currentGameState, heldPiece));

                // The piece has been put into place and locked, check for cleared lines and game over, then if allowed, spawn a new piece.
                completedRows = gameBoard.CheckForClearedLines();

                if (completedRows.Any())
                {
                    StartCoroutine(gameBoard.ClearLines(completedRows));
                    gameState = GameState.ClearingRows;
                }
                else
                {
                    gameState = GameState.SpawnPiece;
                }

                heldPieceThisMove = false;
                break;
            case GameState.ClearingRows:
                if (!gameBoard.clearingRows)
                {
                    gameBoard.LinesCleared(completedRows);
                    gameState = GameState.SpawnPiece;
                    completedRows = new int[0];
                }
                break;
            case GameState.Searching:
                RespondToSearchInput();
                break;
            case GameState.GameOver:
                // Display a "game over", remove piece movement, allow reverse.
                break;
        }

        gameBoard.DrawGameBoard();
        gameBoard.DrawNextPieces(currentPieceBag, nextPieceBag);
        gameBoard.DrawHeldPiece(heldPiece);
    }

    public void GetNextPiece ()
    {
        Piece newPiece = currentPieceBag.First();
        currentPieceBag.RemoveAt(0);

        currentPiece = new ActivePiece(newPiece);

        gameState = GameState.SpawnPiece;

        // If currentPieceBag is empty, copy nextPieceBag over and generate another piece bag.
        if (currentPieceBag.Count == 0)
        {
            currentPieceBag.AddRange(nextPieceBag);
            nextPieceBag = PieceBagManager.GeneratePieceBag();
        }

        gameState = (gameBoard.PlacePiece(currentPiece, pieceIsSpawning: true) ? GameState.MovePiece : GameState.GameOver);
    }

    private void DebugKeysPressed(int keysPressed)
    {
        string pressedKeys = "";
        pressedKeys += ((keysPressed & (int)KeyPressed.Up) == 0 ? " " : "U");
        pressedKeys += ((keysPressed & (int)KeyPressed.Down) == 0 ? " " : "D");
        pressedKeys += ((keysPressed & (int)KeyPressed.Left) == 0 ? " " : "L");
        pressedKeys += ((keysPressed & (int)KeyPressed.Right) == 0 ? " " : "R");
        pressedKeys += ((keysPressed & (int)KeyPressed.SpinLeft) == 0 ? " " : "<");
        pressedKeys += ((keysPressed & (int)KeyPressed.SpinRight) == 0 ? " " : ">");
        pressedKeys += ((keysPressed & (int)KeyPressed.HoldPiece) == 0 ? " " : "H");
        pressedKeys += ((keysPressed & (int)KeyPressed.RotationLeft) == 0 ? " " : "(");
        pressedKeys += ((keysPressed & (int)KeyPressed.RotationRight) == 0 ? " " : ")");
        pressedKeys += ((keysPressed & (int)KeyPressed.Rewind) == 0 ? " " : "«");
        pressedKeys += ((keysPressed & (int)KeyPressed.Forward) == 0 ? " " : "»");
        pressedKeys += ((keysPressed & (int)KeyPressed.Accept) == 0 ? " " : "v");
        pressedKeys += ((keysPressed & (int)KeyPressed.Menu) == 0 ? " " : "M");

        if (!string.IsNullOrWhiteSpace(pressedKeys))
        {
            Debug.Log($"{pressedKeys}");
        }
    }

    public void RespondToGameplayInput()
    {
        int keysPressed = userInput.GetKeysPressed;

        DebugKeysPressed(keysPressed);

        if (TestKey(KeyPressed.Left, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveLeft);
        }
        else if (TestKey(KeyPressed.Right, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveRight);
        }
        else if (TestKey(KeyPressed.Up, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveUp);
        }
        else if (TestKey(KeyPressed.Down, keysPressed))
        {
            if (!gameBoard.MovePiece(currentPiece, _moveDown))
            {
                // If I had sound effects, play a sound effect for the piece locking.
                gameState = GameState.PiecePlaced;
            }
        }
        else if (TestKey(KeyPressed.Accept, keysPressed))
        {
            do { } while (gameBoard.MovePiece(currentPiece, _moveDown));

            if (gameSettings.HardDrop)
            {
                gameState = GameState.PiecePlaced;
            }
        }
        else if (TestKey(KeyPressed.SpinLeft, keysPressed))
        {
            gameBoard.RotatePiece(currentPiece, _rotateLeft);
        }
        else if (TestKey(KeyPressed.SpinRight, keysPressed))
        {
            gameBoard.RotatePiece(currentPiece, _rotateRight);
        }
        else if (TestKey(KeyPressed.HoldPiece, keysPressed))
        {
            if (!heldPieceThisMove)
            {
                heldPieceThisMove = true;

                gameBoard.ClearHeldPiece();

                Piece? tempPiece = null;
                if (heldPiece.HasValue)
                {
                    tempPiece = heldPiece.Value;
                }

                heldPiece = currentPiece.Piece;
                gameBoard.ErasePiece(currentPiece);

                if (tempPiece is not null)
                {
                    currentPiece = new ActivePiece(tempPiece.Value);
                    gameBoard.PlacePiece(currentPiece, true);
                }
                else
                {
                    gameState = GameState.SpawnPiece;
                }
            }
        }
    }

    public void RespondToSearchInput()
    {

    }

    private bool TestKey (KeyPressed key, int? keysPressed)
    {
        return keysPressed is null ? (userInput.GetKeysPressed & (int)key) == (int)key : (keysPressed & (int)key) == (int)key;
    }
}
