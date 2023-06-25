using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEngine : MonoBehaviour
{
    // Reference to the class that maintains board state.
    public GameBoard gameBoard = new GameBoard();

    // previousGameState is for which state to return to after closing the menu.
    public GameState gameState = GameState.None;
    public GameState previousGameState = GameState.None;

    // Player stuff.
    public Settings gameSettings;
    public UserInput userInput;

    // Next seven pieces, and next seven pieces after that.
    public List<Piece> currentPieceBag;
    public List<Piece> nextPieceBag;

    public ActivePiece currentPiece;
    public Piece? heldPiece = null;

    public int lastKeyPressed;

    // Game State, Current Piece, Held Piece?
    public List<Tuple<MinoEnum[,], Piece, Piece?>> gameHistory = new List<Tuple<MinoEnum[,], Piece, Piece?>>();
    private int _searchPosition = -1;

    // Constants for being able to do things.
    private Size _moveLeft = new Size(-1, 0);
    private Size _moveRight = new Size(1, 0);
    private Size _moveUp = new Size(0, 1);
    private Size _moveDown = new Size(0, -1);

    private int _rotateLeft = -1;
    private int _rotateRight = 1;

    // Game state stuff.
    public int linesCleared = 0;
    public int[] completedRows;
    private bool heldPieceThisMove = false;

    private void Awake()
    {
        StartNewGame();

        // Load the settings if they exist, if not this will create a fresh Settings file to modify later.
        userInput.LoadSettingsFromFile();
        userInput.SaveSettingsToFile();
    }

    public void StartNewGame ()
    {
        gameBoard.Initialize();
        gameBoard.DrawGameBoard();

        currentPieceBag = PieceBagManager.GeneratePieceBag();
        nextPieceBag = PieceBagManager.GeneratePieceBag();

        heldPiece = null;
        linesCleared = 0;

        gameHistory = new();

        gameState = GameState.None;
    }

    public void EnqueueGameState(MinoEnum[,] gameState, Piece piece, Piece? held)
        => gameHistory.Add(new Tuple<MinoEnum[,], Piece, Piece?> (gameState, piece, held));

    public Piece GetNextPiece()
    {
        Piece newPiece = currentPieceBag.First();
        currentPieceBag.RemoveAt(0);

        if (!currentPieceBag.Any())
        {
            currentPieceBag.AddRange(nextPieceBag);
            nextPieceBag = PieceBagManager.GeneratePieceBag();
        }

        return newPiece;
    }

    public void Update()
    {
        if (UserInput.TestKey(KeyPressed.Menu, userInput.GetKeysPressed))
        {
            // Then exit this scene and load the title screen.
            SceneManager.LoadScene("TitleScene");
            SceneManager.UnloadSceneAsync("Begin");
        }

        switch (gameState)
        {
            case GameState.None:
                gameState = GameState.SpawnPiece;
                break;
            case GameState.SpawnPiece:
                State_SpawnPiece();
                break;
            case GameState.MovePiece:
                State_MovePiece();

                // In case we've started time traveling, we want to respond immediately instead of on the next frame.
                if (gameState == GameState.Searching)
                {
                    State_Searching();
                }
                break;
            case GameState.PiecePlaced:
                State_PiecePlaced();
                break;
            case GameState.Searching:
                State_Searching();
                break;
            case GameState.ClearingRows:
                if (!gameBoard.clearingRows)
                {
                    gameBoard.LinesCleared(completedRows);
                    gameState = GameState.SpawnPiece;
                    completedRows = new int[0];
                }
                break;
            case GameState.GameOver:
                int keysPressed = userInput.GetKeysPressed;

                if (UserInput.TestKey(KeyPressed.Rewind, keysPressed) ||
                    UserInput.TestKey(KeyPressed.Forward, keysPressed))
                {
                    gameState = GameState.Searching;
                    State_Searching();
                }
                else if (UserInput.TestKey(KeyPressed.Accept, keysPressed))
                {
                    StartNewGame();
                }    
                break;
        }

        gameBoard.DrawGameBoard();
        gameBoard.DrawNextPieces(currentPieceBag, nextPieceBag);
        gameBoard.DrawHeldPiece(heldPiece);
    }

    public void State_SpawnPiece()
    {
        MinoEnum[,] currentGameState = gameBoard.CopyBoardState();

        Piece newPiece = GetNextPiece();

        EnqueueGameState(currentGameState, newPiece, heldPiece);

        currentPiece = new ActivePiece(newPiece);
        gameState = (gameBoard.PlacePiece(currentPiece, true) ? GameState.MovePiece : GameState.GameOver);
    }

    public void State_MovePiece()
    {
        int keysPressed = userInput.GetKeysPressed;

        bool success = MovePiece(keysPressed);
        success = success ? success : RotatePiece(keysPressed);
        success = success ? success : HoldPiece(keysPressed);
        success = success ? success : ShowRotations(keysPressed);
        success = success ? success : TimeTravel(keysPressed);
    }

    #region State_MovePiece methods.
    private bool MovePiece (int keysPressed)
    {
        bool moved = true;

        if (UserInput.TestKey(KeyPressed.Left, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveLeft);
        }
        else if (UserInput.TestKey(KeyPressed.Right, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveRight);
        }
        else if (UserInput.TestKey(KeyPressed.Up, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveUp);
        }
        else if (UserInput.TestKey(KeyPressed.Down, keysPressed))
        {
            if (!gameBoard.MovePiece(currentPiece, _moveDown))
            {
                gameState = GameState.PiecePlaced;
            }
        }
        else if (UserInput.TestKey(KeyPressed.Accept, keysPressed))
        {
            do { } while (gameBoard.MovePiece(currentPiece, _moveDown));

            if (gameSettings.HardDrop)
            {
                gameState = GameState.PiecePlaced;
            }
        }
        else
        {
            moved = false;
        }

        return moved;
    }
    
    private bool RotatePiece (int keysPressed)
    {
        bool rotated = true;

        if (UserInput.TestKey(KeyPressed.SpinLeft, keysPressed))
        {
            gameBoard.RotatePiece(currentPiece, _rotateLeft);
        }
        else if (UserInput.TestKey(KeyPressed.SpinRight, keysPressed))
        {
            gameBoard.RotatePiece(currentPiece, _rotateRight);
        }
        else
        {
            rotated = false;
        }

        return rotated;
    }
 
    private bool HoldPiece (int keysPressed)
    {
        bool swapped = true;

        if (UserInput.TestKey(KeyPressed.HoldPiece, keysPressed))
        {
            if (!heldPieceThisMove)
            {
                heldPieceThisMove = true;

                gameBoard.ClearHeldPiece();

                Piece? tempPiece = null;
                if (heldPiece.HasValue)
                {
                    tempPiece = heldPiece;
                }

                heldPiece = currentPiece.Piece;
                gameBoard.ErasePiece(currentPiece);


                // If tempPiece is null, we did not have a held piece, so proceed as normal.
                if (tempPiece is null)
                {
                    gameState = GameState.SpawnPiece;
                }
                else
                {
                    MinoEnum[,] currentGameState = gameBoard.CopyBoardState();
                    currentPiece = new ActivePiece(tempPiece.Value);
                    EnqueueGameState(currentGameState, currentPiece.Piece, heldPiece);
                    gameBoard.PlacePiece(currentPiece, true);
                }
            }
        }
        else
        {
            swapped = false;
        }

        return swapped;
    }

    private bool ShowRotations (int keysPressed)
    {
        bool rotations = true;

        if (UserInput.TestKey(KeyPressed.RotationLeft, keysPressed))
        {
            // No-op for now.
        }
        else if (UserInput.TestKey(KeyPressed.RotationRight, keysPressed))
        {
            // No-op for now.
        }
        else
        {
            rotations = false;
        }

        return rotations;
    }

    private bool TimeTravel (int keysPressed)
    {
        bool timeTraveling = false;

        if (UserInput.TestKey(KeyPressed.Rewind, keysPressed) ||
            UserInput.TestKey(KeyPressed.Forward, keysPressed))
        {
            if (gameHistory.Count > 0)
            {
                linesCleared = 0;
                _searchPosition = gameHistory.Count - 1;

                gameState = GameState.Searching;
                timeTraveling = false;
            }
        }

        return timeTraveling;
    }
    #endregion

    public void State_PiecePlaced()
    {
        completedRows = gameBoard.CheckForClearedLines();

        if (completedRows.Any())
        {
            linesCleared += completedRows.Length;
            StartCoroutine(gameBoard.ClearLines(completedRows));

            gameState = GameState.ClearingRows;
        }
        else
        {
            gameState = GameState.SpawnPiece;
        }

        heldPieceThisMove = false;
    }

    public void State_Searching()
    {
        int keysPressed = userInput.GetKeysPressed;
        bool rewinding = UserInput.TestKey(KeyPressed.Rewind, keysPressed);
        bool forwarding = UserInput.TestKey(KeyPressed.Forward, keysPressed);
        bool accepting = UserInput.TestKey(KeyPressed.Accept, keysPressed);

        if (!rewinding && !forwarding && !accepting)
        {
            return;
        }

        if (!rewinding && !forwarding && accepting)
        {
            int positionToDelete = _searchPosition + 1;
            int amountToDelete = gameHistory.Count - positionToDelete;

            if (positionToDelete < gameHistory.Count)
            {
                gameHistory.RemoveRange(positionToDelete, amountToDelete);
            }

            gameState = GameState.MovePiece;
            return;
        }

        int currentIndex = _searchPosition;
        int newIndex = -1;

        if (rewinding)
        {
            newIndex = Math.Max (0, currentIndex - 1);
        }
        else if (forwarding)
        {
            newIndex = Math.Min(currentIndex + 1, gameHistory.Count - 1);
        }

        if (currentIndex == newIndex)
        {
            return;
        }

        Tuple<MinoEnum[,], Piece, Piece?> currentBoardState = gameHistory[currentIndex];
        Tuple<MinoEnum[,], Piece, Piece?> newBoardState = gameHistory[newIndex];

        bool sameBoard = BoardStatesEqual(currentIndex, newIndex);

        gameBoard.ErasePiece(currentPiece);
        Piece newPiece;

        if (sameBoard)
        {
            newPiece = ProcessSameBoard(currentBoardState, newBoardState, rewinding);
        }
        else
        {
            newPiece = ProcessDifferentBoard(currentBoardState, newBoardState, rewinding);
        }

        currentPiece = new ActivePiece(newPiece);

        gameBoard.SetGameBoard(newBoardState.Item1);
        gameBoard.PlacePiece(currentPiece, true);

        _searchPosition = newIndex;
    }

    #region State_Searching methods.
    private bool BoardStatesEqual (int currentIndex, int newIndex)
    {
        MinoEnum[,] currentPosition = gameHistory[currentIndex].Item1;
        MinoEnum[,] newPosition = gameHistory[newIndex].Item1;

        for (int x = 0; x < currentPosition.GetLength(0); ++ x)
        {
            for (int y = 0; y < currentPosition.GetLength(1); ++ y)
            {
                if (currentPosition[x, y] != newPosition[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    private Piece ProcessSameBoard(Tuple<MinoEnum[,], Piece, Piece?> currentBoardState,
                                   Tuple<MinoEnum[,], Piece, Piece?> newBoardState,
                                   bool rewinding)
    {
        Piece newPiece;

        if (rewinding)
        {
            // Case 1: no hold piece in the new state.  Push the current piece onto the piece queue.
            if (newBoardState.Item3 is null)
            {
                currentPieceBag.Insert(0, currentBoardState.Item2);
                newPiece = newBoardState.Item2;
            }
            // Case 2: hold piece in the new state.  Swap the pieces.
            else
            {
                newPiece = newBoardState.Item3.Value;
                heldPiece = currentBoardState.Item2;
            }
        }
        else
        {
            if (newBoardState.Item3 is null)    // The board is the same, so we held a piece.  We don't have a held piece now, we do next move.
            {
                newPiece = GetNextPiece();
                heldPiece = currentBoardState.Item2;
            }
            else    // Otherwise, we do have a held piece, so swap hold pieces.
            {
                newPiece = newBoardState.Item2;
                heldPiece = newBoardState.Item3;
            }
        }

        return newPiece;
    }

    private Piece ProcessDifferentBoard(Tuple<MinoEnum[,], Piece, Piece?> currentBoardState,
                                        Tuple<MinoEnum[,], Piece, Piece?> newBoardState,
                                        bool rewinding)
    {
        Piece newPiece;

        if (rewinding)
        {
            currentPieceBag.Insert(0, currentBoardState.Item2);
            newPiece = newBoardState.Item2;
        }
        else
        {
            newPiece = GetNextPiece();
        }

        return newPiece;
    }
    #endregion
}