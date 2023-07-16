using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayfieldScene : MonoBehaviour
{
    // Reference to the class that maintains board state.
    public GameBoard gameBoard;

    // previousGameState is for which state to return to after closing the menu.
    public GameState gameState = GameState.None;
    public GameState previousGameState = GameState.None;

    // Player stuff.
    public Settings gameSettings { get => userInput.gameSettings; }
    public UserInput userInput;

    // Next seven pieces, and next seven pieces after that.
    public List<Piece> currentPieceBag;
    public List<Piece> nextPieceBag;

    public ActivePiece currentPiece;
    public ActivePiece shadowPiece;
    public Piece? heldPiece = null;

    public int ShadowRotationDirection = 0;
    public int ShadowRotationState = 0;
    public bool ShadowRotationActive = false;

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
    [SerializeField] private TMP_Text txt_Lines;
    [SerializeField] private TMP_Text lbl_RotationData;
    [SerializeField] private TMP_Text txt_RotationData;
    [SerializeField] private TMP_Dropdown _currentStyle;
    private string CurrentStyle { get => _currentStyle.options[_currentStyle.value].text; }

    public int linesCleared = 0;
    public int[] completedRows;
    private bool heldPieceThisMove = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        // Load the settings if they exist, if not this will create a fresh Settings file to modify later.
        userInput.LoadSettingsFromFile();
        userInput.SaveSettingsToFile();
        
        StartNewGame();

        PopulateDropdown();
        AssignDropdown();
    }

    private void PopulateDropdown()
    {
        _currentStyle.ClearOptions();
        _currentStyle.AddOptions(Mino.GetCachedStyles());
    }

    private void AssignDropdown()
    {
        TMP_Dropdown.OptionData dropdownValue = _currentStyle.options.FirstOrDefault(data => data.text.Equals(gameSettings.Style, StringComparison.OrdinalIgnoreCase));
        if (dropdownValue != null)
        {
            _currentStyle.value = _currentStyle.options.IndexOf(dropdownValue);
        }
    }

    public void StyleChanged() => gameBoard.SetStyle(CurrentStyle);


    public void StartNewGame ()
    {
        gameBoard = new GameBoard(gameSettings.Style);
        
        gameBoard.MakeBoard(StaticData.editedGameBoard);
        gameBoard.DrawGameBoard();

        StaticData.editedGameBoard = null;

        currentPieceBag = PieceBagManager.GeneratePieceBag();
        nextPieceBag = PieceBagManager.GeneratePieceBag();

        heldPiece = null;
        linesCleared = 0;
        UpdateLinesCleared();

        gameHistory = new();

        lbl_RotationData.alpha = 0;
        txt_RotationData.alpha = 0;
        txt_RotationData.text = "";

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

    public void UpdateLinesCleared ()
    {
        txt_Lines.text = $"Lines {linesCleared}";
    }

    public void Update()
    {
        if (UserInput.TestKey(KeyPressed.Menu, userInput.GetKeysPressed))
        {
            SceneManager.LoadSceneAsync(Constants.GetScene(Constants.Scene.TITLE));
        }

        if (currentPiece is not null &&
            ShadowRotationActive)
        {
            ClearRotationShadow();
        }

        if (shadowPiece is not null)
        {
            ClearPieceShadow(true);
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
                userInput.lockingPieceFrames = 0;

                ClearRotationShadow();
                ClearPieceShadow(true);

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

                if (TimeTravel(keysPressed))
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

        if (shadowPiece is not null)
        {
            gameBoard.DrawPieceShadow(shadowPiece, false);
        }

        shadowPiece = gameBoard.PlummetPiece(currentPiece);

        gameBoard.DrawGameBoard();

        if (currentPiece is not null &&
            shadowPiece is not null &&
            !ShadowRotationActive &&
            currentPiece.CurrentLocation != shadowPiece.CurrentLocation)
        {
            gameBoard.DrawPieceShadow(shadowPiece, true);
        }

        if (currentPiece is not null &&
            ShadowRotationActive)
        {
            gameBoard.DrawRotationShadow(currentPiece, true);
        }

        gameBoard.DrawNextPieces(currentPieceBag, nextPieceBag);
        gameBoard.DrawHeldPiece(heldPiece);
    }

    public void State_SpawnPiece()
    {
        MinoEnum[,] currentGameState = gameBoard.CopyBoardState();

        Piece newPiece = GetNextPiece();

        EnqueueGameState(currentGameState, newPiece, heldPiece);

        currentPiece = MakeActivePiece(newPiece);
        gameState = (gameBoard.PlacePiece(currentPiece, true) ? GameState.MovePiece : GameState.GameOver);
    }

    public ActivePiece MakeActivePiece(Piece newPiece)
    {
        ActivePiece activePiece = new ActivePiece(newPiece);
        activePiece.ShadowRotationDirection = ShadowRotationDirection;
        activePiece.ShadowRotationIndex = (newPiece == Piece.O ? 0 : ShadowRotationState);

        return activePiece;
    }

    public void State_MovePiece()
    {
        int keysPressed = userInput.GetKeysPressed;

        MovePiece(keysPressed);

        if (gameState == GameState.MovePiece)
        {
            RotatePiece(keysPressed);
            HoldPiece(keysPressed);
        }

        ShowRotations(keysPressed);
        TimeTravel(keysPressed);
    }

    #region State_MovePiece methods.
    private bool MovePiece (int keysPressed)
    {
        bool moved = true;
        bool tryLockPiece = false;

        if (UserInput.TestKey(KeyPressed.Left, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveLeft);
        }
        else if (UserInput.TestKey(KeyPressed.Right, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveRight);
        }

        if (UserInput.TestKey(KeyPressed.Up, keysPressed))
        {
            gameBoard.MovePiece(currentPiece, _moveUp);
        }
        else if (UserInput.TestKey(KeyPressed.Down, keysPressed))
        {
            if (!gameBoard.MovePiece(currentPiece, _moveDown))
            {
                if (!UserInput.TestKey(KeyPressed.Down, userInput.lastKeysPressed))
                {
                    gameState = GameState.PiecePlaced;
                }
                else
                {
                    tryLockPiece = true;
                }
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

        if (UserInput.TestKey(KeyPressed.Down, userInput.lastKeysPressed) &&
            UserInput.TestKey(KeyPressed.Down, userInput.currentKeysPressed) &&
            (tryLockPiece || userInput.lockingPieceFrames > 0))
        {
            userInput.lockingPieceFrames++;
        }
        else
        {
            userInput.lockingPieceFrames = 0;
        }

        // If "Accept" was pressed too, then the piece was already locked, and we don't bother with this.
        if ((tryLockPiece && gameState != GameState.PiecePlaced) ||
            userInput.lockingPieceFrames > 0)
        {
            LockIffPieceShouldLock();
        }

        return moved;
    }

    // If we're in this method, the piece is trying to move down, but can't.
    // Therefore, either lock it after enough frames have passed, or ... don't.
    private void LockIffPieceShouldLock()
    {
        if (userInput.lockingPieceFrames >= UserInput.framesBeforeLockingPiece)
        {
            gameState = GameState.PiecePlaced;
            userInput.lockingPieceFrames = 0;
        }
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

                gameBoard.CleanUpGameObjects(gameBoard._heldPiece);

                Piece? tempPiece = null;
                if (heldPiece.HasValue)
                {
                    tempPiece = heldPiece;
                }

                heldPiece = currentPiece.Piece;
                gameBoard.ErasePiece(currentPiece);
                currentPiece = null;

                if (shadowPiece is not null)
                {
                    ClearPieceShadow(true);
                }

                // If tempPiece is null, we did not have a held piece, so proceed as normal.
                if (tempPiece is not null)
                {
                    currentPieceBag.Insert(0, tempPiece.Value);
                }

                gameState = GameState.SpawnPiece;
            }
        }
        else
        {
            swapped = false;
        }

        return swapped;
    }

    private void ShowRotations (int keysPressed)
    {
        int numberOfWallKicks = currentPiece.PieceData.WallKickOffsets[new Tuple<int, int>(0, 1)].GetLength(0);

        if (UserInput.TestKey(KeyPressed.RotationLeft, keysPressed) &&
            UserInput.TestKey(KeyPressed.RotationRight, keysPressed))
        {
            ShadowRotationActive = false;

            lbl_RotationData.alpha = 0;
            txt_RotationData.alpha = 0;
            txt_RotationData.text = "";
        }
        else if (UserInput.TestKey(KeyPressed.RotationLeft, keysPressed))
        {
            ShadowRotationActive = true;

            lbl_RotationData.alpha = 1;
            txt_RotationData.alpha = 1;

            if (ShadowRotationDirection == -1)
            {
                ShadowRotationState = (ShadowRotationState + 1 + numberOfWallKicks) % numberOfWallKicks;
            }
            else
            {
                ShadowRotationDirection = -1;
                ShadowRotationState = 0;
            }
        }
        else if (UserInput.TestKey(KeyPressed.RotationRight, keysPressed))
        {
            ShadowRotationActive = true;

            lbl_RotationData.alpha = 1;
            txt_RotationData.alpha = 1;

            if (ShadowRotationDirection == 1)
            {
                ShadowRotationState = (ShadowRotationState + 1 + numberOfWallKicks) % numberOfWallKicks;
            }
            else
            {
                ShadowRotationDirection = 1;
                ShadowRotationState = 0;
            }
        }

        UpdateRotationText();
        CopyRotationInformation();
    }

    private void UpdateRotationText()
    {
        int numberOfWallKicks = currentPiece.PieceData.WallKickOffsets[new Tuple<int, int>(0, 1)].GetLength(0);

        int curState = currentPiece.RotationState;
        int newState = (curState + ShadowRotationDirection + 4) % 4;

        string direction = (ShadowRotationDirection == -1 ? "counter-clockwise" : (ShadowRotationDirection == 1 ? "clockwise" : "not rotating"));

        txt_RotationData.text = $"Rotating from state {curState} to {newState} [{direction}], checking rotation {currentPiece.ShadowRotationIndex + 1} of {numberOfWallKicks}.";

        var rotationIndex = new Tuple<int, int>(curState, newState);
        if (currentPiece.PieceData.WallKickOffsets.ContainsKey(rotationIndex))
        {
            gameBoard.ErasePiece(currentPiece);

            ActivePiece rotationPiece = currentPiece.Copy();
            rotationPiece.RotationState = newState;

            var wallKicks = currentPiece.PieceData.WallKickOffsets[rotationIndex];

            Size offset = new Size(wallKicks[currentPiece.ShadowRotationIndex, 0], wallKicks[currentPiece.ShadowRotationIndex, 1]);
            rotationPiece.CurrentLocation += offset;

            bool canPlace = gameBoard.ValidatePiecePosition(rotationPiece);

            txt_RotationData.text += $" This {(canPlace ? "is a valid rotation, so the piece will end up here unless an earlier rotation state is also valid" : "collides with something and is an invalid placement, so try the next rotation")}.";

            gameBoard.PlacePiece(currentPiece, true);
        }
    }

    private void CopyRotationInformation()
    {
        if (currentPiece is not null)
        {
            if (currentPiece.Piece == Piece.O)
            {
                ShadowRotationState = 0;
            }

            currentPiece.ShadowRotationDirection = ShadowRotationDirection;
            currentPiece.ShadowRotationIndex = ShadowRotationState;
        }
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
                UpdateLinesCleared();

                _searchPosition = gameHistory.Count - 1;

                gameState = GameState.Searching;
                timeTraveling = false;
            }
        }

        return timeTraveling;
    }
    #endregion

    public void ClearRotationShadow()
    {
        if (ShadowRotationActive && currentPiece is not null)
        {
            gameBoard.DrawRotationShadow(currentPiece, false);
        }
    }

public void ClearPieceShadow(bool removeShadowPiece)
    {
        if (shadowPiece is not null)
        {
            gameBoard.DrawPieceShadow(shadowPiece, false);

            if (removeShadowPiece)
            {
                shadowPiece = null;
            }
        }
    }

    public void State_PiecePlaced()
    {
        completedRows = gameBoard.CheckForClearedLines();

        ClearPieceShadow(true);
        ClearRotationShadow();

        currentPiece = null;

        if (completedRows.Any())
        {
            linesCleared += completedRows.Length;
            UpdateLinesCleared();

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

            heldPieceThisMove = false;

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

        currentPiece = MakeActivePiece(newPiece);

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
                heldPiece = null;
            }
            // Case 2: hold piece in the new state.  Swap the pieces.
            else
            {
                newPiece = newBoardState.Item3.Value;
                heldPiece = newBoardState.Item2;
            }
        }
        else
        {
            if (currentBoardState.Item3 is null)    // The board is the same, so we held a piece.  We don't have a held piece now, we do next move.
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