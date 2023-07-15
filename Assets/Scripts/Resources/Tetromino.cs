using System;
using System.Collections.Generic;
using System.Drawing;

public static class Tetrominos
{
    private static Dictionary<Tuple<int, int>, int[,]> JLSTZWallKickOffsets = new Dictionary<Tuple<int, int>, int[,]>()
    {
        { new Tuple<int, int>(0, 1), new int[5, 2] { { 0, 0 }, { -1, 0 }, { -1,  1 }, { 0, -2 }, { -1, -2 } } },
        { new Tuple<int, int>(1, 0), new int[5, 2] { { 0, 0 }, {  1, 0 }, {  1, -1 }, { 0,  2 }, {  1,  2 } } },
        { new Tuple<int, int>(1, 2), new int[5, 2] { { 0, 0 }, {  1, 0 }, {  1, -1 }, { 0,  2 }, {  1,  2 } } },
        { new Tuple<int, int>(2, 1), new int[5, 2] { { 0, 0 }, { -1, 0 }, { -1,  1 }, { 0, -2 }, { -1, -2 } } },
        { new Tuple<int, int>(2, 3), new int[5, 2] { { 0, 0 }, {  1, 0 }, {  1,  1 }, { 0, -2 }, {  1, -2 } } },
        { new Tuple<int, int>(3, 2), new int[5, 2] { { 0, 0 }, { -1, 0 }, { -1, -1 }, { 0,  2 }, { -1,  2 } } },
        { new Tuple<int, int>(3, 0), new int[5, 2] { { 0, 0 }, { -1, 0 }, { -1, -1 }, { 0,  2 }, { -1,  2 } } },
        { new Tuple<int, int>(0, 3), new int[5, 2] { { 0, 0 }, {  1, 0 }, {  1,  1 }, { 0, -2 }, {  1, -2 } } },
    };

    public static Dictionary<Piece, PieceDefinition> PieceData = new Dictionary<Piece, PieceDefinition>()
    {
        #region Piece.I
        {
            Piece.I, new PieceDefinition()
            {
                Name = Piece.I,
                Sprite = "Block_Cyan_I",
                ThisMino = MinoEnum.I,
                RotationData = new int[4, 4, 4]
                {
                    { { 0, 0, 0, 0 }, { 1, 1, 1, 1 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, },
                    { { 0, 0, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 1, 0 }, { 0, 0, 1, 0 }, },
                    { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 1, 1, 1, 1 }, { 0, 0, 0, 0 }, },
                    { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, }
                },
                WallKickOffsets = new Dictionary<Tuple<int, int>, int[,]>()
                {
                    { new Tuple<int, int>(0, 1), new int[5, 2] { { 0, 0 }, { -2, 0 }, {  1, 0 }, { -2, -1 }, {  1,  2 } } },
                    { new Tuple<int, int>(1, 0), new int[5, 2] { { 0, 0 }, {  2, 0 }, { -1, 0 }, {  2,  1 }, { -1, -2 } } },
                    { new Tuple<int, int>(1, 2), new int[5, 2] { { 0, 0 }, { -1, 0 }, {  2, 0 }, { -1,  2 }, {  2, -1 } } },
                    { new Tuple<int, int>(2, 1), new int[5, 2] { { 0, 0 }, {  1, 0 }, { -2, 0 }, {  1, -2 }, { -2,  1 } } },
                    { new Tuple<int, int>(2, 3), new int[5, 2] { { 0, 0 }, {  2, 0 }, { -1, 0 }, {  2,  1 }, { -1, -2 } } },
                    { new Tuple<int, int>(3, 2), new int[5, 2] { { 0, 0 }, { -2, 0 }, {  1, 0 }, { -2, -1 }, {  1,  2 } } },
                    { new Tuple<int, int>(3, 0), new int[5, 2] { { 0, 0 }, {  1, 0 }, { -2, 0 }, {  1, -2 }, { -2,  1 } } },
                    { new Tuple<int, int>(0, 3), new int[5, 2] { { 0, 0 }, { -1, 0 }, {  2, 0 }, { -1,  2 }, {  2, -1 } } },
                },
                StartingPoint = new Point(4, 21),
            }
        },
        #endregion

        #region Piece.O
        {
            Piece.O, new PieceDefinition()
            {
                Name = Piece.O,
                Sprite = "Block_Yellow_O",
                ThisMino = MinoEnum.O,
                RotationData = new int[4, 3, 4]
                {
                    { { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
                    { { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
                    { { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
                    { { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } },
                },
                // O Pieces do not have a rotation matrix, despite what Puyo Puyo Tetris 2 wants you to think.
                WallKickOffsets = new Dictionary<Tuple<int, int>, int[,]>()
                {
                    { new Tuple<int, int>(0, 1), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(1, 0), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(1, 2), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(2, 1), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(2, 3), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(3, 2), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(3, 0), new int[1, 2] { { 0, 0 } } },
                    { new Tuple<int, int>(0, 3), new int[1, 2] { { 0, 0 } } },
                },
                StartingPoint = new Point(4, 20),
            }
        },
        #endregion

        #region Piece.J
        {
            Piece.J, new PieceDefinition()
            {
                Name = Piece.J,
                Sprite = "Block_Blue_J",
                ThisMino = MinoEnum.J,
                RotationData = new int[4, 3, 3]
                {
                        { { 1, 0, 0 }, { 1, 1, 1 }, { 0, 0, 0 } },
                        { { 0, 1, 1 }, { 0, 1, 0 }, { 0, 1, 0 } },
                        { { 0, 0, 0 }, { 1, 1, 1 }, { 0, 0, 1 } },
                        { { 0, 1, 0 }, { 0, 1, 0 }, { 1, 1, 0 } }
                },
                WallKickOffsets = JLSTZWallKickOffsets,
                StartingPoint = new Point(4, 20),
            }
        },
        #endregion

        #region Piece.L
        {
            Piece.L, new PieceDefinition()
            {
                Name = Piece.L,
                Sprite = "Block_Orange_L",
                ThisMino = MinoEnum.L,
                RotationData = new int[4, 3, 3]
                {
                        { { 0, 0, 1 }, { 1, 1, 1 }, { 0, 0, 0 } },
                        { { 0, 1, 0 }, { 0, 1, 0 }, { 0, 1, 1 } },
                        { { 0, 0, 0 }, { 1, 1, 1 }, { 1, 0, 0 } },
                        { { 1, 1, 0 }, { 0, 1, 0 }, { 0, 1, 0 } },
                },
                WallKickOffsets = JLSTZWallKickOffsets,
                StartingPoint = new Point(4, 20),
            }
        },
        #endregion

        #region Piece.S
        {
            Piece.S, new PieceDefinition()
            {
                Name = Piece.S,
                Sprite = "Block_Green_S",
                ThisMino = MinoEnum.S,
                RotationData = new int[4, 3, 3]
                {
                        { { 0, 1, 1 }, { 1, 1, 0 }, { 0, 0, 0 } },
                        { { 0, 1, 0 }, { 0, 1, 1 }, { 0, 0, 1 } },
                        { { 0, 0, 0 }, { 0, 1, 1 }, { 1, 1, 0 } },
                        { { 1, 0, 0 }, { 1, 1, 0 }, { 0, 1, 0 } }
                },
                WallKickOffsets = JLSTZWallKickOffsets,
                StartingPoint = new Point(4, 20),
            }
        },
        #endregion

        #region Piece.T
        {
            Piece.T, new PieceDefinition()
            {
                Name = Piece.T,
                Sprite = "Block_Purple_T",
                ThisMino = MinoEnum.T,
                RotationData = new int[4, 3, 3]
                {
                        { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 0, 0 } },
                        { { 0, 1, 0 }, { 0, 1, 1 }, { 0, 1, 0 } },
                        { { 0, 0, 0 }, { 1, 1, 1 }, { 0, 1, 0 } },
                        { { 0, 1, 0 }, { 1, 1, 0 }, { 0, 1, 0 } },
                },
                WallKickOffsets = JLSTZWallKickOffsets,
                StartingPoint = new Point(4, 20),
            }
        },
        #endregion

        #region Piece.Z
        {
            Piece.Z, new PieceDefinition()
            {
                Name = Piece.Z,
                Sprite = "Block_Red_Z",
                ThisMino = MinoEnum.Z,
                RotationData = new int[4, 3, 3]
                {
                        { { 1, 1, 0 }, { 0, 1, 1 }, { 0, 0, 0 } },
                        { { 0, 0, 1 }, { 0, 1, 1 }, { 0, 1, 0 } },
                        { { 0, 0, 0 }, { 1, 1, 0 }, { 0, 1, 1 } },
                        { { 0, 1, 0 }, { 1, 1, 0 }, { 1, 0, 0 } },
                },
                WallKickOffsets = JLSTZWallKickOffsets,
                StartingPoint = new Point(4, 20),
            }
        }
        #endregion
    };

    public class PieceDefinition
    {
        public Piece Name { get; set; }
        public MinoEnum ThisMino { get; set; }

        public string Sprite { get; set; }

        public int[,,] RotationData { get; set; }
        public Dictionary<Tuple<int, int>, int[,]> WallKickOffsets { get; set; }
        public Point StartingPoint { get; set; }

        public MinoData MinoData => Mino.GetMinoData(ThisMino);

        public override string ToString()
        {
            string retVal = $"{Name}; {Sprite}\n";

            for (int j = 0; j < RotationData.GetLength(1); ++j)
            {
                for (int i = 0; i < RotationData.GetLength(0); ++i)
                {
                    for (int k = 0; k < RotationData.GetLength(2); ++k)
                    {
                        retVal += (RotationData[i, j, k] == 0 ? ".." : "[]");
                    }

                    retVal += (i == RotationData.GetLength(0) - 1 ? "" : "  |  ");
                }

                retVal += "\n";
            }

            return retVal;
        }
    }

    public static PieceDefinition GetPieceData(Piece p) => PieceData[p];
}

public enum Piece
{
    I,
    J,
    L,
    O,
    S,
    T,
    Z
}

public enum PieceBag
{
    Current,
    Next
}

public class PieceBagManager
{
    public static List<Piece> GeneratePieceBag()
    {
        List<Piece> pieceBag = new List<Piece>() { Piece.I, Piece.J, Piece.L, Piece.O, Piece.S, Piece.T, Piece.Z };

        for (int i = 0; i < pieceBag.Count; ++i)
        {
            int r = i + UnityEngine.Random.Range(0, pieceBag.Count - i);
            Piece swap = pieceBag[r];
            pieceBag[r] = pieceBag[i];
            pieceBag[i] = swap;
        }

        return pieceBag;
    }
}

public class ActivePiece
{
    public Piece Piece { get; set; }
    public Point CurrentLocation { get; set; }
    public int RotationState { get; set; }
    public int ShadowRotationDirection { get; set; } = 0;
    public int ShadowRotationIndex { get; set; } = 0;
    public int ShadowRotationState { get => (RotationState + ShadowRotationDirection + 4) % 4; }

    public Tetrominos.PieceDefinition PieceData => Tetrominos.PieceData[Piece];
    public int[,,] RotationData => Tetrominos.PieceData[Piece].RotationData;

    public ActivePiece (Piece piece)
    {
        Piece = piece;
        CurrentLocation = Tetrominos.GetPieceData(piece).StartingPoint;
        RotationState = 0;
    }

    public ActivePiece Copy()
    {
        return new ActivePiece(Piece)
        {
            Piece = this.Piece,
            CurrentLocation = new Point(this.CurrentLocation.X, this.CurrentLocation.Y),
            RotationState = this.RotationState,
            ShadowRotationIndex = this.ShadowRotationIndex,
            ShadowRotationDirection = this.ShadowRotationDirection
        };
    }
}
