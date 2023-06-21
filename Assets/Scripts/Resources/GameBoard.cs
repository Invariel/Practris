using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static Tetrominos;

public class GameBoard
{
    public const int playfieldWidth = 10;
    public const int playfieldHeight = 40;
    public const int visibleHeight = 23;

    public float spacing = -1f;

    /// <summary>
    /// (0, 0) is the bottom left border piece.
    /// Row 0 is border pieces.
    /// Column 0 is border pieces.
    /// Columns 1 .. n are playable area.
    /// Column n + 1 is border pieces.
    /// </summary>
    public MinoEnum[,] _boardState = new MinoEnum[playfieldWidth + 2, playfieldHeight + 1];
    public GameObject[,] _gameSurface = new GameObject[playfieldWidth + 2, visibleHeight + 1];

    public List<GameObject> _nextPieces = new List<GameObject>();
    public List<GameObject> _heldPiece = new List<GameObject>();

    private const int _rotation = 0;
    private const int _rotRow = 1;
    private const int _rotCol = 2;

    private const int _boardCol = 0;
    private const int _boardRow = 1;

    public bool clearingRows = false;

    // Awake is called before anything happens; may as well be a constructor.
    public void Initialize ()
    {
        for (int x = 0; x < playfieldWidth + 2; ++x)
        {
            for (int y = 0; y < playfieldHeight + 1; ++y)
            {
                try
                {
                    _boardState[x, y] = (x == 0 || x == playfieldWidth + 1 || y == 0) ? MinoEnum.Border : MinoEnum.Empty;

                    if (y < _gameSurface.GetLength(1))
                    {
                        _gameSurface[x, y] = Mino.CreateMino(_boardState[x, y], $"({x,2}, {y,2})", true);

                        if (spacing == -1f)
                        {
                            spacing = _gameSurface[x, y].GetComponent<SpriteRenderer>().bounds.size.x;
                        }
                    }
                }
                catch (Exception _)
                {
                    Debug.LogError($"Error writing block to ({x}, {y}).");
                }
            }
        }
    }
    public MinoEnum[,] CopyBoardState() => (MinoEnum[,])_boardState.Clone();

    public void DrawGameBoard ()
    {
        if (_gameSurface is null)
        {
            return;
        }

        for (int x = 0; x < _gameSurface.GetLength(0); ++ x)
        {
            for (int y = 0; y < _gameSurface.GetLength(1); ++ y)
            {
                Vector3 vector = _gameSurface[x, y].transform.position;
                vector.x = spacing * (x - ((playfieldWidth - 2) / 2));
                vector.y = spacing * (y - (visibleHeight / 2));
                _gameSurface[x, y].transform.position = vector;
            }
        }
    }

    public void ClearNextPieces()
    {
        while (_nextPieces.Any())
        {
            GameObject first = _nextPieces.First();
            _nextPieces.Remove(first);
            MonoBehaviour.Destroy(first);
        }
    }

    public void DrawNextPieces(List<Piece> currentPieceBag, List<Piece> nextPieceBag)
    {
        ClearNextPieces();

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
                            GameObject tile = Mino.CreateMino(pieceData.ThisMino, $"Next {pieceData.Name}", false);

                            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                            sr.color = new UnityEngine.Color(1f, 1f, 1f, 1f - 0.75f * rightSide);

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

    public void ClearHeldPiece()
    {
        while (_heldPiece.Any())
        {
            GameObject first = _heldPiece.First();
            _heldPiece.Remove(first);
            MonoBehaviour.Destroy(first);
        }
    }

    public void DrawHeldPiece(Piece? heldPiece)
    {
        ClearHeldPiece();

        if (!heldPiece.HasValue)
        {
            return;
        }

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
                    GameObject tile = Mino.CreateMino(pieceData.ThisMino, $"Held {pieceData.Name}", false);

                    Vector3 vector = tile.transform.position;
                    vector.x = spacing / 2 * (leftTile + column);
                    vector.y = spacing / 2 * (topTile - row);
                    tile.transform.position = vector;

                    _heldPiece.Add(tile);
                }
            }
        }
    }

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

    public bool PlacePiece (ActivePiece currentPiece, bool pieceIsSpawning)
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
                    int locX = currentPiece.CurrentLocation.X + column;
                    int locY = currentPiece.CurrentLocation.Y - row;

                    // If the rotation data says the space is empty, do nothing.
                    if (rotationData[rotation, row, column] == 0)
                    {
                        continue;
                    }

                    _boardState[locX, locY] = currentPiece.PieceData.ThisMino;

                    if (locX < _gameSurface.GetLength(_boardCol) && locY < _gameSurface.GetLength(_boardRow))
                    {
                        var sr = _gameSurface[locX, locY].GetComponent<SpriteRenderer>();
                        sr.sprite = Resources.Load<Sprite>(currentPiece.PieceData.MinoData.MinoSprite);
                    }
                }
            }
        }

        return canPlacePiece;
    }

    public void EraseMino (int row, int column)
    {
        _boardState[column, row] = MinoEnum.Empty;
        _gameSurface[column, row].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(Mino.GetMinoData(MinoEnum.Empty).MinoSprite);
    }

    public void ErasePiece (ActivePiece currentPiece)
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
                    sr.sprite = Resources.Load<Sprite>(Mino.GetMinoData(MinoEnum.Empty).MinoSprite);
                }
            }
        }
    }

    // If a "down" move is false, then lock the piece and spawn a new one.
    public bool MovePiece (ActivePiece currentPiece, Size movement)
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

    public void RotatePiece (ActivePiece currentPiece, int rotationDirection)
    {
        bool validPlacement = false;

        ErasePiece(currentPiece);

        int initialRotation = currentPiece.RotationState;
        currentPiece.RotationState = (currentPiece.RotationState + rotationDirection + 4) % 4;

        int[,] wallKickData = currentPiece.PieceData.WallKickOffsets.First(i => i.Key.Item1 == initialRotation && i.Key.Item2 == currentPiece.RotationState).Value;

        // If I had sound effects, this is where I would play a sound effect.

        for (int i = 0; i < wallKickData.GetLength(_rotation) && !validPlacement; ++ i)
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

        for (int row = 1; row < _boardState.GetLength(_boardRow); ++ row)
        {
            for (int col = 0; col < _boardState.GetLength(_boardCol); ++ col)
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
        for (int col = 1; col < _boardState.GetLength(_boardCol) - 1; ++ col)
        {
            foreach (int row in rows)
            {
                for (int r = row; r < _boardState.GetLength(_boardRow) - 1; ++ r)
                {
                    _boardState[col, r] = _boardState[col, r + 1];

                    if (r < _gameSurface.GetLength(_boardRow))
                    {
                        _gameSurface[col, r].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(Mino.GetMinoData(_boardState[col, r + 1]).MinoSprite);
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
                DrawGameBoard();
                yield return new WaitForSeconds(0.01f);
            }
        }
        clearingRows = false;
    }
}