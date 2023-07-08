using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace System.Runtime.CompilerServices
{
    class IsExternalInit { }
}

public class GameBoard
{
    public int playfieldWidth { get; private set; } = 10;
    public int playfieldHeight { get; private set; } = 40;
    public int visibleHeight { get; private set; } = 23;

    private float spacing = -1f;

    public string _style = "v2";

    /// <summary>
    /// (0, 0) is the bottom left border piece.
    /// (1, 1) is the bottom left playable piece.
    /// Column 0, column playfieldWidth + 1, and row 0 are border pieces.
    /// </summary>
    public MinoEnum[,] _boardState;
    public GameObject[,] _gameSurface;

    public List<GameObject> _nextPieces = new List<GameObject>();
    public List<GameObject> _heldPiece = new List<GameObject>();

    private const int _rotation = 0;
    private const int _rotRow = 1;
    private const int _rotCol = 2;

    private const int _boardCol = 0;
    private const int _boardRow = 1;

    public bool clearingRows = false;

    public GameBoard()
    {
        _boardState = new MinoEnum[playfieldWidth + 2, playfieldHeight + 1];
        _gameSurface = new GameObject[playfieldWidth + 2, visibleHeight + 1];
        _style = "Original";
    }

    public GameBoard (string style)
    {
        _boardState = new MinoEnum[playfieldWidth + 2, playfieldHeight + 1];
        _gameSurface = new GameObject[playfieldWidth + 2, visibleHeight + 1];
        _style = style;
    }

    public MinoEnum[,] CopyBoardState() => (MinoEnum[,])_boardState.Clone();

#nullable enable
    public void MakeBoard(MinoEnum[,]? boardState)
    {
        if (boardState is null)
        {
            _boardState = new MinoEnum[playfieldWidth + 2, playfieldHeight + 1];
            _gameSurface = new GameObject[playfieldWidth + 2, visibleHeight + 1];

            InitializeBoard();
        }
        else
        {
            _boardState = boardState;
            _gameSurface = new GameObject[_boardState.GetLength(0), visibleHeight + 1];
        }
    }
#nullable disable

    private void InitializeBoard()
    {
        for (int x = 0; x < _boardState.GetLength(0); ++ x)
        {
            for (int y = 0; y < _boardState.GetLength(1); ++ y)
            {
                try
                {
                    _boardState[x, y] = (x == 0 || x == _boardState.GetLength(0) - 1 || y == 0) ? MinoEnum.Border : MinoEnum.Empty;
                }
                catch (Exception)
                {
                    Debug.LogError($"Error writing to ({x}, {y})");
                }
            }
        }
    }

    private void InitializeGameSurface()
    {
        if (_boardState is null)
        {
            return;
        }

        for (int x = 0; x < _gameSurface.GetLength(0); ++ x)
        {
            for (int y = 0; y < _gameSurface.GetLength(1); ++ y)
            {
                if (_gameSurface[x, y] is GameObject obj)
                {
                    MonoBehaviour.Destroy(obj);
                }
            }
        }

        _gameSurface = new GameObject[playfieldWidth + 2, visibleHeight + 1];

        for (int x = 0; x < _gameSurface.GetLength(0); ++ x)
        {
            for (int y = 0; y < _gameSurface.GetLength(1); ++ y)
            {
                if (_gameSurface[x, y] is null)
                {
                    _gameSurface[x, y] = Mino.CreateMino(_boardState[x, y], _style, $"({x,2}, {y,2})", true);
                }

                if (spacing == -1f)
                {
                    spacing = _gameSurface[x, y].GetComponent<SpriteRenderer>().bounds.size.x;
                }

                // I _should_ only need to do this once, because this puts the sprites onto the visual surface.  I hope.
                Vector3 vector = _gameSurface[x, y].transform.position;
                vector.x = spacing * (x - ((playfieldWidth - 2) / 2));
                vector.y = spacing * (y - (visibleHeight / 2));
                _gameSurface[x, y].transform.position = vector;
            }    
        }
    }

    public void SetGameBoard(MinoEnum[,] newGameBoard)
    {
        if (_boardState.GetLength(0) != newGameBoard.GetLength(0) ||
            _boardState.GetLength(1) != newGameBoard.GetLength(1))
        {
            playfieldWidth = newGameBoard.GetLength(0);
            playfieldHeight = newGameBoard.GetLength(1);

            InitializeGameSurface();
        }

        for (int x = 0; x < newGameBoard.GetLength(0); ++x)
        {
            for (int y = 0; y < newGameBoard.GetLength(1); ++y)
            {
                _boardState[x, y] = newGameBoard[x, y];
            }
        }
    }

    public void DrawGameBoard()
    {
        if (_gameSurface[0, 0] is null)
        {
            InitializeGameSurface();
        }

        for (int x = 0; x < _gameSurface.GetLength(0); ++ x)
        {
            for (int y = 0; y < _gameSurface.GetLength(1); ++ y)
            {
                string spriteFilename = Mino.GetSpriteFilename(_boardState[x, y], _style);
                _gameSurface[x, y].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteFilename);
            }
        }
    }

    #region Held Piece, CurrentPieceBag and NextPieceBag display and cleanup.
    public void CleanUpGameObjects (List<GameObject> gameObjects)
    {
        while (gameObjects.Any())
        {
            GameObject first = gameObjects.First();
            gameObjects.Remove(first);
            MonoBehaviour.Destroy(first);
        }
    }

    public void DrawHeldPiece(Piece? heldPiece)
    {
        CleanUpGameObjects(_heldPiece);

        if (heldPiece.HasValue)
        {
            float leftTile = playfieldWidth / 2 - 20;
            float topTile = visibleHeight / 2 - 1;

            var pieceData = Tetrominos.GetPieceData(heldPiece.Value);
            var rotationData = pieceData.RotationData;

            for (int row = 0; row < rotationData.GetLength(_rotRow); ++row)
            {
                for (int column = 0; column < rotationData.GetLength(_rotCol); ++column)
                {
                    if (rotationData[0, row, column] == 1)
                    {
                        GameObject tile = Mino.CreateMino(pieceData.ThisMino, _style, $"Held {pieceData.Name}", false);

                        Vector3 vector = tile.transform.position;
                        vector.x = spacing / 2 * (leftTile + column);
                        vector.y = spacing / 2 * (topTile - row);
                        tile.transform.position = vector;

                        _heldPiece.Add(tile);
                    }
                }
            }
        }
    }

    public void DrawNextPieces(List<Piece> currentPieceBag, List<Piece> nextPieceBag)
    {
        CleanUpGameObjects(_nextPieces);

        float leftTile = playfieldWidth / 2 + 10;
        float topTile = visibleHeight / 2 - 1;

        int rightSide = -1;

        foreach (List<Piece> current in new List<List<Piece>>() { currentPieceBag, nextPieceBag })
        {
            leftTile += 6;
            ++rightSide;

            for (int bagSlot = 0; bagSlot < current.Count; ++bagSlot)
            {
                var pieceData = Tetrominos.GetPieceData(current[bagSlot]);
                var rotationData = pieceData.RotationData;

                for (int row = 0; row < rotationData.GetLength(_rotRow); ++row)
                {
                    for (int column = 0; column < rotationData.GetLength(_rotCol); ++column)
                    {
                        if (rotationData[0, row, column] == 1)
                        {
                            GameObject tile = Mino.CreateMino(pieceData.ThisMino, _style, $"Next {pieceData.Name}", false);

                            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                            sr.color = new UnityEngine.Color(1f, 1f, 1f, (rightSide == 0 ? 1f : 0.2f));

                            Vector3 vector = tile.transform.position;
                            vector.x = spacing / 2 * (leftTile + column);
                            vector.y = spacing / 2 * (topTile - row - (bagSlot * 3));
                            tile.transform.position = vector;

                            _nextPieces.Add(tile);
                        }
                    }
                }
            }
        }
    }
    #endregion

    public bool ValidatePiecePosition(ActivePiece currentPiece, Point? newPosition = null)
    {
        bool validPlacement = true;

        Point location = newPosition ?? currentPiece.CurrentLocation;
        var rotationData = currentPiece.RotationData;
        var rotation = currentPiece.RotationState;

        for (int row = 0; row < rotationData.GetLength(_rotRow) && validPlacement; row++)
        {
            for (int column = 0; column < rotationData.GetLength(_rotCol) && validPlacement; column++)
            {
                if (rotationData[currentPiece.RotationState, row, column] == 0)
                {
                    continue;
                }

                if ((location.X + column <= 0 || location.X + column >= _boardState.GetLength(_boardCol) ||
                    location.Y - row <= 0 || location.Y - row >= _boardState.GetLength(_boardRow)))
                {
                    if (rotationData[currentPiece.RotationState, row, column] == 0)
                    {
                        continue;
                    }
                    else
                    {
                        validPlacement = false;
                        continue;
                    }
                }


                if (_boardState[location.X + column, location.Y - row] != MinoEnum.Empty)
                {
                    Debug.Log($"Piece collision at ({location.X + row}, {location.Y})");
                    validPlacement = false;
                }
            }
        }

        return validPlacement;
    }

    public bool PlacePiece(ActivePiece currentPiece, bool pieceIsSpawning)
    {
        bool canPlacePiece = ValidatePiecePosition(currentPiece);

        if (pieceIsSpawning || canPlacePiece)
        {
            var rotationData = currentPiece.RotationData;
            var rotation = currentPiece.RotationState;

            // Regardless of whether the piece can be placed or not, I want to draw the piece in.  This is because top outs show the final piece colliding with the board.
            for (int row = 0; row < rotationData.GetLength(_rotRow); ++row)
            {
                for (int column = 0; column < rotationData.GetLength(_rotCol); ++column)
                {
                    // If the rotation data says the space is empty, do nothing.
                    if (rotationData[rotation, row, column] == 0)
                    {
                        continue;
                    }

                    int locX = currentPiece.CurrentLocation.X + column;
                    int locY = currentPiece.CurrentLocation.Y - row;

                    _boardState[locX, locY] = currentPiece.PieceData.ThisMino;

                    if (locX < _gameSurface.GetLength(_boardCol) && locY < _gameSurface.GetLength(_boardRow))
                    {
                        var sr = _gameSurface[locX, locY].GetComponent<SpriteRenderer>();

                        string filename = Mino.GetSpriteFilename(currentPiece.PieceData.ThisMino, _style);
                        sr.sprite = Resources.Load<Sprite>(filename);
                    }
                }
            }
        }

        return canPlacePiece;
    }

    public void DrawPieceShadow (ActivePiece shadow, bool draw)
    {
        var rotationData = shadow.RotationData;
        var rotation = shadow.RotationState;

        // Maybe refactor the piece-drawing code to accept a colour or whatever.
        for (int row = 0; row < rotationData.GetLength(_rotRow); ++row)
        {
            for (int column = 0; column < rotationData.GetLength(_rotCol); ++ column)
            {
                if (rotationData[rotation, row, column] == 0)
                {
                    continue;
                }

                int locX = shadow.CurrentLocation.X + column;
                int locY = shadow.CurrentLocation.Y - row;

                if (locX < _gameSurface.GetLength(_boardCol) && locY < _gameSurface.GetLength(_boardRow) && _boardState[locX, locY] == MinoEnum.Empty)
                {
                    var sr = _gameSurface[locX, locY].GetComponent<SpriteRenderer>();
                    string filename = draw ?
                        Mino.GetShadowSpriteFilename(shadow.PieceData.ThisMino, _style) :
                        Mino.GetSpriteFilename(MinoEnum.Empty, _style);
                    sr.sprite = Resources.Load<Sprite>(filename);
                }
            }
        }
    }

    public void DrawRotationShadow (ActivePiece piece, bool draw)
    {
        var rotationData = piece.RotationData;
        var rotation = (piece.RotationState + piece.ShadowRotationDirection + 4) % 4;
        var wallKicks = piece.PieceData.WallKickOffsets;

        int from = piece.RotationState;
        int to = (from + piece.ShadowRotationDirection + 4) % 4;

        var desiredWallKick = new Tuple<int, int>(from, to);

        // If this key doesn't exist, from == to, or we have bad data.  Do not draw shadows.
        if (!wallKicks.ContainsKey(desiredWallKick))
        {
            return;
        }

        var wallKick = wallKicks[new Tuple<int, int>(from, to)];

        for (int row = 0; row < rotationData.GetLength(_rotRow); ++ row)
        {
            for (int column = 0; column < rotationData.GetLength(_rotCol); ++ column)
            {
                if (rotationData[rotation, row, column] == 0)
                {
                    continue;
                }

                int locX = piece.CurrentLocation.X + column + wallKick[piece.ShadowRotationIndex, 0];
                int locY = piece.CurrentLocation.Y - row + wallKick[piece.ShadowRotationIndex, 1];

                if (locX >= 0 && locX < _gameSurface.GetLength(_boardCol) &&
                    locY >= 0 && locY < _gameSurface.GetLength(_boardRow) &&
                    _boardState[locX, locY] == MinoEnum.Empty)
                {
                    var sr = _gameSurface[locX, locY].GetComponent<SpriteRenderer>();
                    string filename = draw ?
                        Mino.GetShadowSpriteFilename(piece.PieceData.ThisMino, _style) :
                        Mino.GetSpriteFilename(MinoEnum.Empty, _style);
                    sr.sprite = Resources.Load<Sprite>(filename);
                }
            }
        }
    }

    public void EraseMino(int row, int column)
    {
        _boardState[column, row] = MinoEnum.Empty;
        string filename = Mino.GetSpriteFilename(MinoEnum.Empty, _style);
        _gameSurface[column, row].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(filename);
    }

    public void ErasePiece(ActivePiece currentPiece)
    {
        var rotationData = currentPiece.RotationData;
        var rotation = currentPiece.RotationState;

        // Regardless of whether the piece can be placed or not, I want to draw the piece in.  This is because top outs show the final piece colliding with the board.
        for (int row = 0; row < rotationData.GetLength(_rotRow); ++row)
        {
            for (int column = 0; column < rotationData.GetLength(_rotCol); ++column)
            {
                int locX = currentPiece.CurrentLocation.X + column;
                int locY = currentPiece.CurrentLocation.Y - row;

                // If the rotation data says the space is empty, do nothing.
                if (rotationData[rotation, row, column] == 0)
                {
                    continue;
                }

                _boardState[locX, locY] = MinoEnum.Empty;

                if (locX < _gameSurface.GetLength(0) && locY < _gameSurface.GetLength(1))
                {
                    SpriteRenderer sr = _gameSurface[locX, locY].GetComponent<SpriteRenderer>();
                    string filename = Mino.GetSpriteFilename(MinoEnum.Empty, _style);
                    sr.sprite = Resources.Load<Sprite>(filename);
                }
            }
        }
    }

    // If a "down" move is false, then lock the piece and spawn a new one.
    public bool MovePiece(ActivePiece currentPiece, Size movement)
    {
        bool moved = false;

        ErasePiece(currentPiece);
        Point newLocation = currentPiece.CurrentLocation + movement;

        if (ValidatePiecePosition(currentPiece, newLocation))
        {
            // If I had sound effects, this is where I would play a sound effect.
            currentPiece.CurrentLocation = newLocation;
            moved = true;
        }

        PlacePiece(currentPiece, false);

        return moved;
    }

    public ActivePiece PlummetPiece(ActivePiece currentPiece)
    {
        if (currentPiece is null)
        {
            return null;
        }

        ActivePiece newPiece = currentPiece.Copy();

        ErasePiece(currentPiece);

        Size movement = new Size(0, 0);
        bool canMove = false;

        do
        {
            movement += new Size(0, -1);
            canMove = ValidatePiecePosition(currentPiece, currentPiece.CurrentLocation + movement);
        } while (canMove);

        movement += new Size(0, 1);

        PlacePiece(currentPiece, false);

        newPiece.CurrentLocation += movement;
        return newPiece;
    }

    public void RotatePiece(ActivePiece currentPiece, int rotationDirection)
    {
        bool validPlacement = false;

        ErasePiece(currentPiece);

        int initialRotation = currentPiece.RotationState;
        currentPiece.RotationState = (currentPiece.RotationState + rotationDirection + 4) % 4;

        int[,] wallKickData = currentPiece.PieceData.WallKickOffsets.First(i => i.Key.Item1 == initialRotation && i.Key.Item2 == currentPiece.RotationState).Value;

        // If I had sound effects, this is where I would play a sound effect.

        for (int i = 0; i < wallKickData.GetLength(_rotation) && !validPlacement; ++i)
        {
            Size movement = new Size(wallKickData[i, 0], wallKickData[i, 1]);
            Point newLocation = currentPiece.CurrentLocation + movement;

            if (ValidatePiecePosition(currentPiece, newLocation))
            {
                currentPiece.CurrentLocation = newLocation;
                validPlacement = true;
            }
        }

        if (!validPlacement)
        {
            currentPiece.RotationState = initialRotation;
        }

        PlacePiece(currentPiece, false);
    }

    public int[] CheckForClearedLines()
    {
        List<int> fullRows = new List<int>();

        for (int row = 1; row < _boardState.GetLength(_boardRow); ++row)
        {
            for (int col = 0; col < _boardState.GetLength(_boardCol); ++col)
            {
                if (_boardState[col, row] == MinoEnum.Empty)
                {
                    break;
                }

                if (col == _boardState.GetLength(_boardCol) - 1)
                {
                    fullRows.Add(row);
                }
            }
        }

        fullRows.Reverse();
        return fullRows.ToArray();
    }

    public void LinesCleared(int[] rows)
    {
        for (int col = 1; col < _boardState.GetLength(_boardCol) - 1; ++col)
        {
            foreach (int row in rows)
            {
                for (int r = row; r < _boardState.GetLength(_boardRow) - 1; ++r)
                {
                    _boardState[col, r] = _boardState[col, r + 1];

                    if (r < _gameSurface.GetLength(_boardRow))
                    {
                        string filename = Mino.GetSpriteFilename(_boardState[col, r + 1], _style);
                        _gameSurface[col, r].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(filename);
                    }
                }

                _boardState[col, _boardState.GetLength(_boardRow) - 1] = MinoEnum.Empty;
            }
        }
    }

    public IEnumerator ClearLines(int[] rows)
    {
        clearingRows = true;
        // Erase rows, starting from the largest number, pausing between each.
        for (int col = 1; col < _boardState.GetLength(_boardCol) - 1; ++col)
        {
            foreach (int row in rows)
            {
                EraseMino(row, col);
            }

            DrawGameBoard();
            yield return new WaitForSeconds(0.01f);
        }
        clearingRows = false;
    }
}