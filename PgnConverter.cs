using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFishAnalyzer
{
        #region PgnConverter
        public class PgnConverter
        {
            #region PGN Conversion Helper Classes
            public class PgnMove
            {
                public int fromCol { get; set; }
                public int fromRow { get; set; }
                public int toCol { get; set; }
                public int toRow { get; set; }
                public string piece { get; set; }
            }
            public class Square
            {
                public int col { get; set; }
                public int row { get; set; }
                public Square(int r, int c)
                {
                    this.row = r;
                    this.col = c;
                }
            }

            public class EnpassantChecks
            {
                public List<CheckSquare> CheckSquares { get; set; }
                public string EnPassantLocation { get; set; }
            }

            public class CheckSquare
            {
                public int row { get; set; }
                public int col { get; set; }
                public CheckSquare(int r, int c)
                {
                    row = r;
                    col = c;
                }
            }
            #endregion

            #region PGN Conversion Static Lookup Hashes
            public static HashSet<char> SkipChars = new HashSet<char>() { '!', '?', '+', '#', '*' };

            public static Dictionary<string, EnpassantChecks> CanEnPassantMovesWhite = new Dictionary<string, EnpassantChecks>() {
                { "a2a4", new EnpassantChecks() { EnPassantLocation = "a3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 1) } } },
                { "b2b4", new EnpassantChecks() { EnPassantLocation = "b3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 0), new CheckSquare(4, 2) } } },
                { "c2c4", new EnpassantChecks() { EnPassantLocation = "c3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 1), new CheckSquare(4, 3) } } },
                { "d2d4", new EnpassantChecks() { EnPassantLocation = "d3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 2), new CheckSquare(4, 4) } } },
                { "e2e4", new EnpassantChecks() { EnPassantLocation = "e3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 3), new CheckSquare(4, 5) } } },
                { "f2f4", new EnpassantChecks() { EnPassantLocation = "f3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 4), new CheckSquare(4, 6) } } },
                { "g2g4", new EnpassantChecks() { EnPassantLocation = "g3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 5), new CheckSquare(4, 7) } } },
                { "h2h4", new EnpassantChecks() { EnPassantLocation = "h3", CheckSquares = new List<CheckSquare>() { new CheckSquare(4, 6) } } },
            };

            public static Dictionary<string, EnpassantChecks> CanEnPassantMovesBlack = new Dictionary<string, EnpassantChecks>() {
                { "a7a5", new EnpassantChecks() { EnPassantLocation = "a6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 1) } } },
                { "b7b5", new EnpassantChecks() { EnPassantLocation = "b6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 0), new CheckSquare(3, 2) } } },
                { "c7c5", new EnpassantChecks() { EnPassantLocation = "c6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 1), new CheckSquare(3, 3) } } },
                { "d7d5", new EnpassantChecks() { EnPassantLocation = "d6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 2), new CheckSquare(3, 4) } } },
                { "e7e5", new EnpassantChecks() { EnPassantLocation = "e6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 3), new CheckSquare(3, 5) } } },
                { "f7f4", new EnpassantChecks() { EnPassantLocation = "f6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 4), new CheckSquare(3, 6) } } },
                { "g7g5", new EnpassantChecks() { EnPassantLocation = "g6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 5), new CheckSquare(3, 7) } } },
                { "h7h5", new EnpassantChecks() { EnPassantLocation = "h6", CheckSquares = new List<CheckSquare>() { new CheckSquare(3, 6) } } },
            };

            public static Dictionary<int, char> Cols = new Dictionary<int, char>() { { 0, 'a' }, { 1, 'b' }, { 2, 'c' }, { 3, 'd' }, { 4, 'e' }, { 5, 'f' }, { 6, 'g' }, { 7, 'h' } };
            public static Dictionary<int, char> Rows = new Dictionary<int, char>() { { 7, '1' }, { 6, '2' }, { 5, '3' }, { 4, '4' }, { 3, '5' }, { 2, '6' }, { 1, '7' }, { 0, '8' } };

            public static Dictionary<char, int> ColsChar = new Dictionary<char, int>() { { 'a', 0 }, { 'b', 1 }, { 'c', 2 }, { 'd', 3 }, { 'e', 4 }, { 'f', 5 }, { 'g', 6 }, { 'h', 7 } };
            public static Dictionary<char, int> RowsChar = new Dictionary<char, int>() { { '1', 7 }, { '2', 6 }, { '3', 5 }, { '4', 4 }, { '5', 3 }, { '6', 2 }, { '7', 1 }, { '8', 0 } };

            Dictionary<int, HashSet<int>> fastDiagonalLookups = new Dictionary<int, HashSet<int>>() {{ 0, new HashSet<int>() { 9, 18, 27, 36, 45, 54, 63, } },
                { 1, new HashSet<int>() { 8, 10, 19, 28, 37, 46, 55, } },
                { 2, new HashSet<int>() { 9, 16, 11, 20, 29, 38, 47, } },
                { 3, new HashSet<int>() { 10, 17, 24, 12, 21, 30, 39, } },
                { 4, new HashSet<int>() { 11, 18, 25, 32, 13, 22, 31, } },
                { 5, new HashSet<int>() { 12, 19, 26, 33, 40, 14, 23, } },
                { 6, new HashSet<int>() { 13, 20, 27, 34, 41, 48, 15, } },
                { 7, new HashSet<int>() { 14, 21, 28, 35, 42, 49, 56, } },
                { 8, new HashSet<int>() { 1, 17, 26, 35, 44, 53, 62, } },
                { 9, new HashSet<int>() { 0, 16, 2, 18, 27, 36, 45, 54, 63, } },
                { 10, new HashSet<int>() { 1, 17, 24, 3, 19, 28, 37, 46, 55, } },
                { 11, new HashSet<int>() { 2, 18, 25, 32, 4, 20, 29, 38, 47, } },
                { 12, new HashSet<int>() { 3, 19, 26, 33, 40, 5, 21, 30, 39, } },
                { 13, new HashSet<int>() { 4, 20, 27, 34, 41, 48, 6, 22, 31, } },
                { 14, new HashSet<int>() { 5, 21, 28, 35, 42, 49, 56, 7, 23, } },
                { 15, new HashSet<int>() { 6, 22, 29, 36, 43, 50, 57, } },
                { 16, new HashSet<int>() { 9, 2, 25, 34, 43, 52, 61, } },
                { 17, new HashSet<int>() { 8, 24, 10, 3, 26, 35, 44, 53, 62, } },
                { 18, new HashSet<int>() { 9, 0, 25, 32, 11, 4, 27, 36, 45, 54, 63, } },
                { 19, new HashSet<int>() { 10, 1, 26, 33, 40, 12, 5, 28, 37, 46, 55, } },
                { 20, new HashSet<int>() { 11, 2, 27, 34, 41, 48, 13, 6, 29, 38, 47, } },
                { 21, new HashSet<int>() { 12, 3, 28, 35, 42, 49, 56, 14, 7, 30, 39, } },
                { 22, new HashSet<int>() { 13, 4, 29, 36, 43, 50, 57, 15, 31, } },
                { 23, new HashSet<int>() { 14, 5, 30, 37, 44, 51, 58, } },
                { 24, new HashSet<int>() { 17, 10, 3, 33, 42, 51, 60, } },
                { 25, new HashSet<int>() { 16, 32, 18, 11, 4, 34, 43, 52, 61, } },
                { 26, new HashSet<int>() { 17, 8, 33, 40, 19, 12, 5, 35, 44, 53, 62, } },
                { 27, new HashSet<int>() { 18, 9, 0, 34, 41, 48, 20, 13, 6, 36, 45, 54, 63, } },
                { 28, new HashSet<int>() { 19, 10, 1, 35, 42, 49, 56, 21, 14, 7, 37, 46, 55, } },
                { 29, new HashSet<int>() { 20, 11, 2, 36, 43, 50, 57, 22, 15, 38, 47, } },
                { 30, new HashSet<int>() { 21, 12, 3, 37, 44, 51, 58, 23, 39, } },
                { 31, new HashSet<int>() { 22, 13, 4, 38, 45, 52, 59, } },
                { 32, new HashSet<int>() { 25, 18, 11, 4, 41, 50, 59, } },
                { 33, new HashSet<int>() { 24, 40, 26, 19, 12, 5, 42, 51, 60, } },
                { 34, new HashSet<int>() { 25, 16, 41, 48, 27, 20, 13, 6, 43, 52, 61, } },
                { 35, new HashSet<int>() { 26, 17, 8, 42, 49, 56, 28, 21, 14, 7, 44, 53, 62, } },
                { 36, new HashSet<int>() { 27, 18, 9, 0, 43, 50, 57, 29, 22, 15, 45, 54, 63, } },
                { 37, new HashSet<int>() { 28, 19, 10, 1, 44, 51, 58, 30, 23, 46, 55, } },
                { 38, new HashSet<int>() { 29, 20, 11, 2, 45, 52, 59, 31, 47, } },
                { 39, new HashSet<int>() { 30, 21, 12, 3, 46, 53, 60, } },
                { 40, new HashSet<int>() { 33, 26, 19, 12, 5, 49, 58, } },
                { 41, new HashSet<int>() { 32, 48, 34, 27, 20, 13, 6, 50, 59, } },
                { 42, new HashSet<int>() { 33, 24, 49, 56, 35, 28, 21, 14, 7, 51, 60, } },
                { 43, new HashSet<int>() { 34, 25, 16, 50, 57, 36, 29, 22, 15, 52, 61, } },
                { 44, new HashSet<int>() { 35, 26, 17, 8, 51, 58, 37, 30, 23, 53, 62, } },
                { 45, new HashSet<int>() { 36, 27, 18, 9, 0, 52, 59, 38, 31, 54, 63, } },
                { 46, new HashSet<int>() { 37, 28, 19, 10, 1, 53, 60, 39, 55, } },
                { 47, new HashSet<int>() { 38, 29, 20, 11, 2, 54, 61, } },
                { 48, new HashSet<int>() { 41, 34, 27, 20, 13, 6, 57, } },
                { 49, new HashSet<int>() { 40, 56, 42, 35, 28, 21, 14, 7, 58, } },
                { 50, new HashSet<int>() { 41, 32, 57, 43, 36, 29, 22, 15, 59, } },
                { 51, new HashSet<int>() { 42, 33, 24, 58, 44, 37, 30, 23, 60, } },
                { 52, new HashSet<int>() { 43, 34, 25, 16, 59, 45, 38, 31, 61, } },
                { 53, new HashSet<int>() { 44, 35, 26, 17, 8, 60, 46, 39, 62, } },
                { 54, new HashSet<int>() { 45, 36, 27, 18, 9, 0, 61, 47, 63, } },
                { 55, new HashSet<int>() { 46, 37, 28, 19, 10, 1, 62, } },
                { 56, new HashSet<int>() { 49, 42, 35, 28, 21, 14, 7, } },
                { 57, new HashSet<int>() { 48, 50, 43, 36, 29, 22, 15, } },
                { 58, new HashSet<int>() { 49, 40, 51, 44, 37, 30, 23, } },
                { 59, new HashSet<int>() { 50, 41, 32, 52, 45, 38, 31, } },
                { 60, new HashSet<int>() { 51, 42, 33, 24, 53, 46, 39, } },
                { 61, new HashSet<int>() { 52, 43, 34, 25, 16, 54, 47, } },
                { 62, new HashSet<int>() { 53, 44, 35, 26, 17, 8, 55, } },
                { 63, new HashSet<int>() { 54, 45, 36, 27, 18, 9, 0, } },
            };
            #endregion

            #region PGN Conversion Internal State 
            public List<List<string>> chessBoard = new List<List<string>>()
            {
                new List<string>() { "r", "n", "b", "q", "k", "b", "n", "r" },
                new List<string>() { "p", "p", "p", "p", "p", "p", "p", "p" },
                new List<string>() { "", "", "", "", "", "", "", "" },
                new List<string>() { "", "", "", "", "", "", "", "" },
                new List<string>() { "", "", "", "", "", "", "", "" },
                new List<string>() { "", "", "", "", "", "", "", "" },
                new List<string>() { "P", "P", "P", "P", "P", "P", "P", "P" },
                new List<string>() { "R", "N", "B", "Q", "K", "B", "N", "R" },
            };
            public bool internalWhiteLostCastleRightsKingside { get; set; }
            public bool internalWhiteLostCastleRightsQueenside { get; set; }
            public bool internalBlackLostCastleRightsKingside { get; set; }
            public bool internalBlackLostCastleRightsQueenside { get; set; }
            public string enPassantLocation = "-";// - indicates not allowed.
            public bool internalIsBlacksMove { get; set; }
            public int internalMoveCount { get; set; }
            public int internalHalfMovesSinceCaptureOrPawnCount { get; set; }
            public int GetMoveInCurrentPosition()
            {
                return (internalMoveCount / 2) + 1;
            }
            public StringBuilder internalPgnSb { get; set; }
            #endregion

            #region PGN Conversion Functionality
            /// <summary>
            /// This is a way to shorthand retrieve an entire conversion from Smith Notation to PGN.
            /// </summary>
            /// <param name="smithNotation"></param>
            /// <returns></returns>
            public string GetPGNFromSmithNotation(string smithNotation)
            {
                try
                {
                    string[] moves = smithNotation.Split(' ');
                    foreach (var m in moves)
                    {
                        MakeNextSmithMove(m);
                    }
                }
                catch { }
                return internalPgnSb.ToString();
            }

            /// <summary>
            /// Converts the current board state into a FEN position.
            /// </summary>
            /// <returns></returns>
            public string GetFENPosition()
            {
                StringBuilder fenSB = new StringBuilder();
                //rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR b KQkq - 0 1
                for (int r = 0; r < 8; r++)
                {
                    int emptyPawnCounter = 0;
                    for (int c = 0; c < 8; c++)
                    {
                        string piece = chessBoard[r][c];
                        if (piece == "")
                        {
                            emptyPawnCounter++;
                        }
                        else
                        {
                            if (emptyPawnCounter > 0)
                            {
                                fenSB.Append(emptyPawnCounter);
                                emptyPawnCounter = 0;
                            }
                            fenSB.Append(piece);
                        }
                    }
                    if (emptyPawnCounter > 0)
                    {
                        fenSB.Append(emptyPawnCounter);
                    }
                    if (r != 7)
                    {
                        fenSB.Append("/");
                    }
                }
                if (internalIsBlacksMove)
                {
                    fenSB.Append(" b ");
                }
                else
                {
                    fenSB.Append(" w ");
                }
                bool hasCastleRights = false;
                if (!internalWhiteLostCastleRightsKingside)
                {
                    hasCastleRights = true;
                    fenSB.Append("K");
                }
                if (!internalWhiteLostCastleRightsQueenside)
                {
                    hasCastleRights = true;
                    fenSB.Append("Q");
                }
                if (!internalBlackLostCastleRightsKingside)
                {
                    hasCastleRights = true;
                    fenSB.Append("k");
                }
                if (!internalBlackLostCastleRightsQueenside)
                {
                    hasCastleRights = true;
                    fenSB.Append("q");
                }
                if (!hasCastleRights)
                {
                    fenSB.Append("-");
                }
                fenSB.Append(" ");
                fenSB.Append(enPassantLocation);
                fenSB.Append(" ");
                fenSB.Append(internalHalfMovesSinceCaptureOrPawnCount.ToString());
                fenSB.Append(" ");
                fenSB.Append(GetMoveInCurrentPosition().ToString());
                return fenSB.ToString();
            }

            List<List<string>> chessBoardIDs { get; set; }
            Dictionary<char, HashSet<string>> uniquePieceIDs { get; set; }
            Dictionary<string, Square> pieceLocations { get; set; }
            int nextID { get; set; }

            /// <summary>
            /// The following function makes internal moves one at a time using smith notation, this is to assist in producing FEN positions and PGN conversions for each move made.
            /// </summary>
            /// <param name="m"></param>
            public void MakeNextSmithMove(string m)
            {
                try
                {
                    if (internalPgnSb == null)
                        internalPgnSb = new StringBuilder();
                    Queue<PgnMove> makeMoves = new Queue<PgnMove>();
                    string from = m.Substring(0, 2);
                    string to = m.Substring(2, 2);
                    int fromCol = ((int)m[0] - (int)'a');
                    int fromRow = 7 - ((int)m[1] - (int)'1');
                    int toCol = (int)m[2] - (int)'a';
                    int toRow = 7 - ((int)m[3] - (int)'1');
                    string movePiece = chessBoard[fromRow][fromCol];
                    string toPiece = chessBoard[toRow][toCol];

                    if (movePiece == "p" || movePiece == "P" || toPiece != "")
                    {
                        // only resets when a pawn is moved or when a piece is captured:
                        internalHalfMovesSinceCaptureOrPawnCount = 0;
                    }
                    else
                    {
                        internalHalfMovesSinceCaptureOrPawnCount++;
                    }

                    if (from == "a8")
                        internalBlackLostCastleRightsQueenside = true;
                    if (from == "a1")
                        internalWhiteLostCastleRightsQueenside = true;
                    if (from == "h8")
                        internalBlackLostCastleRightsKingside = true;
                    if (from == "h1")
                        internalWhiteLostCastleRightsKingside = true;

                    // check en passant rights for FEN generation:
                    enPassantLocation = "-";
                    if (movePiece == "P")
                    {
                        if (CanEnPassantMovesWhite.ContainsKey(m))
                        {
                            foreach (var c in CanEnPassantMovesWhite[m].CheckSquares)
                            {
                                if (chessBoard[c.row][c.col] == "p")
                                {
                                    // can en passant:
                                    enPassantLocation = CanEnPassantMovesWhite[m].EnPassantLocation;
                                    break;
                                }
                            }
                        }
                    }
                    if (movePiece == "p")
                    {
                        if (CanEnPassantMovesBlack.ContainsKey(m))
                        {
                            foreach (var c in CanEnPassantMovesBlack[m].CheckSquares)
                            {
                                if (chessBoard[c.row][c.col] == "P")
                                {
                                    // can en passant:
                                    enPassantLocation = CanEnPassantMovesBlack[m].EnPassantLocation;
                                    break;
                                }
                            }
                        }
                    }

                    // check castle:
                    string nextMove = "";
                    if (movePiece == "k")
                    {
                        internalBlackLostCastleRightsKingside = true;
                        internalBlackLostCastleRightsQueenside = true;
                        if (fromCol == 4 && fromRow == 0)
                        {
                            if (toCol == 6 && toRow == 0)
                            {
                                nextMove = "0-0";
                                makeMoves.Enqueue(new PgnMove() { fromRow = 0, fromCol = 7, toRow = 0, toCol = 5, piece = "r" });
                            }
                            if (toCol == 2 && toRow == 0)
                            {
                                nextMove = "0-0-0";
                                makeMoves.Enqueue(new PgnMove() { fromRow = 0, fromCol = 0, toRow = 0, toCol = 3, piece = "r" });
                            }
                        }
                    }
                    if (movePiece == "K")
                    {
                        internalWhiteLostCastleRightsKingside = true;
                        internalWhiteLostCastleRightsQueenside = true;
                        if (fromCol == 4 && fromRow == 7)
                        {
                            if (toCol == 6 && toRow == 7)
                            {
                                nextMove = "0-0";
                                makeMoves.Enqueue(new PgnMove() { fromRow = 7, fromCol = 7, toRow = 7, toCol = 5, piece = "R" });
                            }
                            if (toCol == 2 && toRow == 7)
                            {
                                nextMove = "0-0-0";
                                makeMoves.Enqueue(new PgnMove() { fromRow = 7, fromCol = 0, toRow = 7, toCol = 3, piece = "R" });
                            }
                        }
                    }
                    if (nextMove == "")
                    {
                        bool isCapture = toPiece != "";
                        if (movePiece == "P" || movePiece == "p")
                        {
                            if (isCapture)
                                nextMove = Cols[fromCol] + "x" + to;
                            else
                                nextMove = to;
                        }
                        else
                        {
                            switch (movePiece)
                            {
                                case "R":
                                case "r":
                                    nextMove = "R" + from + (isCapture ? "x" : "") + to;
                                    break;
                                case "Q":
                                case "q":
                                    nextMove = "Q" + from + (isCapture ? "x" : "") + to;
                                    break;
                                case "B":
                                case "b":
                                    nextMove = "B" + from + (isCapture ? "x" : "") + to;
                                    break;
                                case "N":
                                case "n":
                                    nextMove = "N" + from + (isCapture ? "x" : "") + to;
                                    break;
                                case "K":
                                case "k":
                                    nextMove = "K" + (isCapture ? "x" : "") + to;
                                    break;
                            }
                        }
                        char upgradePiece = ' ';
                        if (m.Length > 4 && (movePiece == "P" || movePiece == "p"))
                        {
                            upgradePiece = m[4];
                            switch (upgradePiece)
                            {
                                case 'q':
                                case 'Q':
                                    nextMove += "=Q";
                                    if (movePiece == "P")
                                        movePiece = "Q";
                                    else
                                        movePiece = "q";
                                    break;
                                case 'n':
                                case 'N':
                                    nextMove += "=N";
                                    if (movePiece == "P")
                                        movePiece = "N";
                                    else
                                        movePiece = "n";
                                    break;
                                case 'b':
                                case 'B':
                                    nextMove += "=B";
                                    if (movePiece == "P")
                                        movePiece = "B";
                                    else
                                        movePiece = "b";
                                    break;
                                case 'r':
                                case 'R':
                                    nextMove += "=R";
                                    if (movePiece == "P")
                                        movePiece = "R";
                                    else
                                        movePiece = "r";
                                    break;
                            }
                        }
                    }
                    chessBoard[fromRow][fromCol] = "";
                    chessBoard[toRow][toCol] = movePiece;
                    while (makeMoves.Count > 0)
                    {
                        var makeMove = makeMoves.Dequeue();
                        chessBoard[makeMove.fromRow][makeMove.fromCol] = "";
                        chessBoard[makeMove.toRow][makeMove.toCol] = makeMove.piece;
                    }
                    if (internalMoveCount % 2 == 0)
                    {
                        if (internalMoveCount > 0)
                        {
                            internalPgnSb.Append(" ");
                        }
                        internalPgnSb.Append(((internalMoveCount / 2) + 1).ToString() + ". ");
                    }
                    else
                    {
                        internalPgnSb.Append(" ");
                    }
                    internalPgnSb.Append(nextMove);
                    internalMoveCount++;
                    internalIsBlacksMove = !internalIsBlacksMove;
                }
                catch { }
            }

            internal void InternalMakeNextPgnMove(char piece, int fromRow, int fromCol, int toRow, int toCol, string upgrade, List<string> smithNotationMoves)
            {
                string smithMove = Cols[fromCol].ToString() + Rows[fromRow].ToString() + Cols[toCol].ToString() + Rows[toRow].ToString() + upgrade;
                string pieceAtToLocation = chessBoard[toRow][toCol];
                chessBoard[fromRow][fromCol] = "";
                chessBoard[toRow][toCol] = piece.ToString();

                // need to remove unique IDs from circulation if it is a unique piece:
                string toID = chessBoardIDs[toRow][toCol];
                if (toID != "")
                {
                    this.pieceLocations.Remove(toID);
                    this.uniquePieceIDs[pieceAtToLocation[0]].Remove(toID);
                }

                string fromID = chessBoardIDs[fromRow][fromCol];
                if (fromID != "")
                {
                    // update new location:
                    this.pieceLocations[fromID].row = toRow;
                    this.pieceLocations[fromID].col = toCol;
                }

                chessBoardIDs[toRow][toCol] = chessBoardIDs[fromRow][fromCol];
                chessBoardIDs[fromRow][fromCol] = "";

                if (upgrade != "")
                {
                    string newID = nextID.ToString();
                    this.pieceLocations[newID] = new Square(toRow, toCol);
                    this.uniquePieceIDs[upgrade[0]].Add(newID);
                    nextID++;
                }

                smithNotationMoves.Add(smithMove);
            }

            internal bool InternalCanPieceMoveToLocation(char piece, int fromRow, int fromCol, int toRow, int toCol)
            {
                if (piece == 'b' || piece == 'B')
                {
                    int key1 = fromRow * 8 + fromCol;
                    int key2 = toRow * 8 + toCol;
                    bool isOnDiagonal = fastDiagonalLookups.ContainsKey(key1) && fastDiagonalLookups[key1].Contains(key2);
                    if (!isOnDiagonal)
                    {
                        // not in same diagonal, impossible for bishop to move to square
                        return false;
                    }
                    while (toRow != fromRow || toCol != fromCol)
                    {
                        if (fromRow < toRow)
                            fromRow++;
                        else if (fromRow > toRow)
                            fromRow--;
                        if (fromCol < toCol)
                            fromCol++;
                        else if (fromCol > toCol)
                            fromCol--;

                        if (toRow != fromRow || toCol != fromCol)
                        {
                            if (chessBoard[fromRow][fromCol] != "")
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                if (piece == 'r' || piece == 'R')
                {
                    bool isOnRowOrCol = (toRow == fromRow || toCol == fromCol);
                    if (!isOnRowOrCol)
                    {
                        // not in same row or col, impossible for rook to move to square
                        return false;
                    }
                    while (toRow != fromRow || toCol != fromCol)
                    {
                        if (fromRow < toRow)
                            fromRow++;
                        else if (fromRow > toRow)
                            fromRow--;
                        if (fromCol < toCol)
                            fromCol++;
                        else if (fromCol > toCol)
                            fromCol--;
                        if (toRow != fromRow || toCol != fromCol)
                        {
                            if (chessBoard[fromRow][fromCol] != "")
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                if (piece == 'q' || piece == 'Q')
                {
                    int key1 = fromRow * 8 + fromCol;
                    int key2 = toRow * 8 + toCol;
                    bool isOnDiagonal = fastDiagonalLookups.ContainsKey(key1) && fastDiagonalLookups[key1].Contains(key2);
                    bool isOnRowOrCol = (toRow == fromRow || toCol == fromCol);
                    if (!isOnRowOrCol && !isOnDiagonal)
                    {
                        // not in same row, col or diagonal, impossible for queen to move to square
                        return false;
                    }
                    while (toRow != fromRow || toCol != fromCol)
                    {
                        if (fromRow < toRow)
                            fromRow++;
                        else if (fromRow > toRow)
                            fromRow--;
                        if (fromCol < toCol)
                            fromCol++;
                        else if (fromCol > toCol)
                            fromCol--;

                        if (toRow != fromRow || toCol != fromCol)
                        {
                            if (chessBoard[fromRow][fromCol] != "")
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                if (piece == 'n' || piece == 'N')
                {
                    return
                        ((fromRow + 2 == toRow) && (fromCol + 1 == toCol)) ||
                        ((fromRow + 2 == toRow) && (fromCol - 1 == toCol)) ||
                        ((fromRow - 2 == toRow) && (fromCol + 1 == toCol)) ||
                        ((fromRow - 2 == toRow) && (fromCol - 1 == toCol)) ||
                        ((fromRow + 1 == toRow) && (fromCol + 2 == toCol)) ||
                        ((fromRow + 1 == toRow) && (fromCol - 2 == toCol)) ||
                        ((fromRow - 1 == toRow) && (fromCol + 2 == toCol)) ||
                        ((fromRow - 1 == toRow) && (fromCol - 2 == toCol));
                }
                if (piece == 'k' || piece == 'K')
                {
                    return true;
                }
                return false;
            }

            internal bool InternalIsKingInCheckAfterMove(char piece, int fromRow, int fromCol, int toRow, int toCol, bool isWhitesTurn)
            {
                // King cant move into check:
                if (piece == 'k' || piece == 'K')
                    return false;

                int kingRow = -1;
                int kingCol = -1;

                if (isWhitesTurn)
                {
                    kingRow = pieceLocations["13"].row;
                    kingCol = pieceLocations["13"].col;
                }
                else
                {
                    kingRow = pieceLocations["5"].row;
                    kingCol = pieceLocations["5"].col;
                }

                bool isOnRowOrCol = (kingRow == fromRow || kingCol == fromCol);
                int key1 = fromRow * 8 + fromCol;
                int key2 = kingRow * 8 + kingCol;
                bool isOnDiagonal = fastDiagonalLookups.ContainsKey(key1) && fastDiagonalLookups[key1].Contains(key2);

                // Determine which path of the board to check for potential pins:
                int rowWalk = kingRow < fromRow ? 1 : (kingRow == fromRow ? 0 : -1);
                int colWalk = kingCol < fromCol ? 1 : (kingCol == fromCol ? 0 : -1);

                if (isOnRowOrCol || isOnDiagonal)
                {
                    kingRow += rowWalk;
                    kingCol += colWalk;
                    while (kingRow >= 0 && kingRow <= 7 && kingCol >= 0 && kingCol <= 7)
                    {
                        if (kingRow == toRow && kingCol == toCol)
                            return false; // moving the piece a place still protecting the king.

                        if (!(kingRow == fromRow && kingCol == fromCol)) // skip the from position since it will be blank.
                        {
                            // check for potential pins from rooks, bishops and queens:
                            if (isWhitesTurn)
                            {
                                if ((chessBoard[kingRow][kingCol] == "b" && isOnDiagonal) ||
                                    (chessBoard[kingRow][kingCol] == "r" && isOnRowOrCol) ||
                                    chessBoard[kingRow][kingCol] == "q")
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((chessBoard[kingRow][kingCol] == "B" && isOnDiagonal) ||
                                    (chessBoard[kingRow][kingCol] == "R" && isOnRowOrCol) ||
                                    chessBoard[kingRow][kingCol] == "Q")
                                {
                                    return true;
                                }
                            }
                            // found some other piece on the board that can't check the king, short circuit:
                            if (chessBoard[fromRow][fromCol] != "")
                                return false;
                        }
                        kingRow += rowWalk;
                        kingCol += colWalk;
                    }
                }
                return false;
            }

            /// <summary>
            /// This function is defined to very quickly parse PGNs and convert them into smith notation that can be fed to engines.
            /// 
            /// In tests it currently can parse roughly 25000 lines per second which is fast enough to run on server side parsing.
            /// 
            /// It will search all subvariations of any PGN passed in and give back all lines within the PGN converted to smith.
            /// </summary>
            /// <param name="pgnNotation">Any number of PGNs as a single string object</param>
            /// <returns>A list of a list of smith notation moves</returns>
            public List<List<string>> GetSmithNotationFromPGN(string pgnNotation)
            {
                List<List<string>> gamesFound = GetFullLinesFromPGN(pgnNotation);
                List<List<string>> gamesConverted = new List<List<string>>();

                foreach (var game in gamesFound)
                {
                    List<string> smithNotationMoves = new List<string>();

                    try
                    {
                        // Reset the board states:
                        this.chessBoard = new List<List<string>>()
                        {
                            new List<string>() { "r", "n", "b", "q", "k", "b", "n", "r" },
                            new List<string>() { "p", "p", "p", "p", "p", "p", "p", "p" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "P", "P", "P", "P", "P", "P", "P", "P" },
                            new List<string>() { "R", "N", "B", "Q", "K", "B", "N", "R" },
                        };
                        this.chessBoardIDs = new List<List<string>>()
                        {
                            new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "", "", "", "", "", "", "", "" },
                            new List<string>() { "9", "10", "11", "12", "13", "14", "15", "16" },
                        };
                        this.uniquePieceIDs = new Dictionary<char, HashSet<string>>()
                        {
                            { 'r', new HashSet<string>() { "1", "8" } },
                            { 'n', new HashSet<string>() { "2", "7" } },
                            { 'b', new HashSet<string>() { "3", "6" } },
                            { 'q', new HashSet<string>() { "4" } },
                            { 'k', new HashSet<string>() { "5" } },
                            { 'R', new HashSet<string>() { "9", "16" } },
                            { 'N', new HashSet<string>() { "10", "15" } },
                            { 'B', new HashSet<string>() { "11", "14" } },
                            { 'Q', new HashSet<string>() { "12" } },
                            { 'K', new HashSet<string>() { "13" } },
                        };
                        this.pieceLocations = new Dictionary<string, Square>() {
                            { "1", new Square(0,0) },
                            { "2", new Square(0,1) },
                            { "3", new Square(0,2) },
                            { "4", new Square(0,3) },
                            { "5", new Square(0,4) },
                            { "6", new Square(0,5) },
                            { "7", new Square(0,6) },
                            { "8", new Square(0,7) },
                            { "9", new Square(7,0) },
                            { "10", new Square(7,1) },
                            { "11", new Square(7,2) },
                            { "12", new Square(7,3) },
                            { "13", new Square(7,4) },
                            { "14", new Square(7,5) },
                            { "15", new Square(7,6) },
                            { "16", new Square(7,7) },
                        };
                        this.nextID = 17;
                        bool isWhitesTurn = false;
                        foreach (var m in game)
                        {
                            if (smithNotationMoves.Count >= 55)
                            {

                            }

                            isWhitesTurn = !isWhitesTurn;

                            #region Castling Short Circuits
                            bool isCastleKingside = m == "O-O";
                            if (isCastleKingside)
                            {
                                if (isWhitesTurn)
                                {
                                    chessBoard[7][4] = "";
                                    chessBoard[7][5] = "R";
                                    chessBoard[7][6] = "K";
                                    chessBoard[7][7] = "";

                                    chessBoardIDs[7][4] = "";
                                    chessBoardIDs[7][5] = "16";
                                    chessBoardIDs[7][6] = "13";
                                    chessBoardIDs[7][7] = "";

                                    this.pieceLocations["16"].row = 7;
                                    this.pieceLocations["16"].col = 5;
                                    this.pieceLocations["13"].row = 7;
                                    this.pieceLocations["13"].col = 6;

                                    smithNotationMoves.Add("e1g1");
                                }
                                else
                                {
                                    chessBoard[0][4] = "";
                                    chessBoard[0][5] = "8";
                                    chessBoard[0][6] = "5";
                                    chessBoard[0][7] = "";

                                    chessBoardIDs[0][4] = "";
                                    chessBoardIDs[0][5] = "8";
                                    chessBoardIDs[0][6] = "5";
                                    chessBoardIDs[0][7] = "";

                                    this.pieceLocations["8"].row = 0;
                                    this.pieceLocations["8"].col = 5;
                                    this.pieceLocations["5"].row = 0;
                                    this.pieceLocations["5"].col = 6;
                                    smithNotationMoves.Add("e8g8");
                                }
                                continue;
                            }
                            bool isCastleQueenside = m == "O-O-O";
                            if (isCastleQueenside)
                            {
                                if (isWhitesTurn)
                                {
                                    chessBoard[7][4] = "";
                                    chessBoard[7][3] = "R";
                                    chessBoard[7][2] = "K";
                                    chessBoard[7][0] = "";

                                    chessBoardIDs[7][4] = "";
                                    chessBoardIDs[7][3] = "9";
                                    chessBoardIDs[7][2] = "13";
                                    chessBoardIDs[7][0] = "";

                                    this.pieceLocations["9"].row = 7;
                                    this.pieceLocations["9"].col = 3;
                                    this.pieceLocations["13"].row = 7;
                                    this.pieceLocations["13"].col = 2;

                                    smithNotationMoves.Add("e1c1");
                                }
                                else
                                {
                                    chessBoard[0][4] = "";
                                    chessBoard[0][3] = "r";
                                    chessBoard[0][2] = "k";
                                    chessBoard[0][0] = "";

                                    chessBoardIDs[0][4] = "";
                                    chessBoardIDs[0][3] = "1";
                                    chessBoardIDs[0][2] = "5";
                                    chessBoardIDs[0][0] = "";

                                    this.pieceLocations["1"].row = 0;
                                    this.pieceLocations["1"].col = 3;
                                    this.pieceLocations["5"].row = 0;
                                    this.pieceLocations["5"].col = 2;

                                    smithNotationMoves.Add("e8c8");
                                }
                                continue;
                            }
                            #endregion

                            #region Find Piece Being Moved
                            char piece = m[0];
                            if ('a' <= piece && piece <= 'h')
                            {
                                if (!isWhitesTurn)
                                {
                                    piece = 'p';
                                }
                                else
                                {
                                    piece = 'P';
                                }
                            }
                            else
                            {
                                if (!isWhitesTurn)
                                {
                                    switch (piece)
                                    {
                                        case 'B':
                                            piece = 'b';
                                            break;
                                        case 'R':
                                            piece = 'r';
                                            break;
                                        case 'N':
                                            piece = 'n';
                                            break;
                                        case 'Q':
                                            piece = 'q';
                                            break;
                                        case 'K':
                                            piece = 'k';
                                            break;
                                    }
                                }
                            }
                            #endregion

                            #region Pawn Short Circuits
                            bool isCapture = m.Contains("x");
                            if (piece == 'p' || piece == 'P')
                            {
                                bool isUpgrade = m.Contains("=");
                                string upgrade = "";
                                if (isUpgrade)
                                {
                                    upgrade = m[m.Length - 1].ToString();
                                }
                                if (isCapture)
                                {
                                    int fromCol = ColsChar[m[0]];
                                    int toRow = RowsChar[m[3]];
                                    int toCol = ColsChar[m[2]];
                                    int fromRow = toRow;
                                    if (isWhitesTurn)
                                    {
                                        fromRow++;
                                    }
                                    else
                                    {
                                        fromRow--;
                                    }
                                    InternalMakeNextPgnMove(piece, fromRow, fromCol, toRow, toCol, upgrade, smithNotationMoves);
                                    continue;
                                }
                                else
                                {
                                    int toRow = RowsChar[m[1]];
                                    int toCol = ColsChar[m[0]];
                                    int fromRow = toRow;
                                    int fromCol = toCol;
                                    if (isWhitesTurn)
                                    {
                                        if (chessBoard[toRow + 1][toCol] == "P")
                                        {
                                            fromRow = toRow + 1;
                                        }
                                        else
                                        {
                                            fromRow = toRow + 2;
                                        }
                                    }
                                    else
                                    {
                                        if (chessBoard[toRow - 1][toCol] == "p")
                                        {
                                            fromRow = toRow - 1;
                                        }
                                        else
                                        {
                                            fromRow = toRow - 2;
                                        }
                                    }
                                    InternalMakeNextPgnMove(piece, fromRow, fromCol, toRow, toCol, upgrade, smithNotationMoves);
                                    continue;
                                }
                            }
                            #endregion

                            #region Remaining Piece Moves
                            // Three options:
                            // 3 chars - only contains the piece, and the to square
                            // 4 chars - only contains the piece, the from col, and the to square
                            // 5 chars - contains the piece, the from square, and the to square

                            // Replace captures:
                            string mp = m.Replace("x", "");
                            bool moveFound = false;
                            if (mp.Length == 5)
                            {
                                int toRow = RowsChar[mp[4]];
                                int toCol = ColsChar[mp[3]];
                                int fromRow = RowsChar[mp[2]];
                                int fromCol = ColsChar[mp[1]];
                                InternalMakeNextPgnMove(piece, fromRow, fromCol, toRow, toCol, "", smithNotationMoves);
                                continue;
                            }
                            else if (mp.Length == 4)
                            {
                                int toRow = RowsChar[mp[3]];
                                int toCol = ColsChar[mp[2]];
                                int fromRow = -1; // Need to find this.
                                int fromCol = ColsChar[mp[1]];
                                // Just scan the IDs and find the one in the col given, there can only be one:
                                foreach (var id in uniquePieceIDs[piece])
                                {
                                    if (pieceLocations[id].col == fromCol)
                                    {
                                        // Must also check that moving the piece does not place the king in check.
                                        bool isKingInCheckAfterMove = InternalIsKingInCheckAfterMove(piece, pieceLocations[id].row, pieceLocations[id].col, toRow, toCol, isWhitesTurn);
                                        if (!isKingInCheckAfterMove)
                                        {
                                            fromRow = pieceLocations[id].row;
                                            InternalMakeNextPgnMove(piece, fromRow, fromCol, toRow, toCol, "", smithNotationMoves);
                                            moveFound = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (mp.Length == 3)
                            {
                                int toRow = RowsChar[mp[2]];
                                int toCol = ColsChar[mp[1]];
                                int fromRow = -1; // Need to find this.
                                int fromCol = -1; // Need to find this.

                                // This is the hard part, when given no from information, we need to figure out if a piece can move to the to square...
                                // Must also check that moving the piece does not place the king in check.
                                foreach (var id in uniquePieceIDs[piece])
                                {
                                    // First check if a piece can move to a location.
                                    bool canPieceMoveToLocation = InternalCanPieceMoveToLocation(piece, pieceLocations[id].row, pieceLocations[id].col, toRow, toCol);
                                    if (canPieceMoveToLocation)
                                    {
                                        // Must also check that moving the piece does not place the king in check.
                                        bool isKingInCheckAfterMove = InternalIsKingInCheckAfterMove(piece, pieceLocations[id].row, pieceLocations[id].col, toRow, toCol, isWhitesTurn);
                                        if (!isKingInCheckAfterMove)
                                        {
                                            fromCol = pieceLocations[id].col;
                                            fromRow = pieceLocations[id].row;
                                            InternalMakeNextPgnMove(piece, fromRow, fromCol, toRow, toCol, "", smithNotationMoves);
                                            moveFound = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (moveFound)
                                continue;

                            // Should never make it here, all paths short circuit:
                            throw new Exception("Invalid PGN cannot parse.");
                            #endregion
                        }
                    }
                    catch (Exception ex) 
                    { 
                    }

                    // if moves have been found, 
                    if (smithNotationMoves.Count > 0)
                    {
                        gamesConverted.Add(smithNotationMoves);
                    }
                }
                return gamesConverted;
            }

            /// <summary>
            /// PGN parsing is broken into two steps, first step is to take the input clear irrelevant data, and find all lines and sublines,
            /// each subline found creates an entire game record that return from move one to the end of the line.
            /// </summary>
            /// <param name="pgnNotation"></param>
            /// <returns></returns>
            public List<List<string>> GetFullLinesFromPGN(string pgnNotation)
            {
                var pgnLines = pgnNotation.Replace("\r", "").Split('\n');
                StringBuilder cleanLine = new StringBuilder();
                List<string> linesParse = new List<string>();
                List<List<string>> linesRet = new List<List<string>>();
                Stack<List<string>> pgnGameStack = null;
                foreach (var pgnLine in pgnLines)
                {
                    var parseLine = pgnLine.Trim();
                    if (!parseLine.StartsWith("1. "))
                    {
                        continue; // only parse pgns lines that contain an actual game.
                    }
                    char lastLastLastChar = 'x';
                    char lastLastChar = 'x';
                    char lastChar = 'x';
                    Stack<char> skipStack = new Stack<char>();
                    cleanLine.Clear();
                    foreach (var i in parseLine)
                    {
                        if (SkipChars.Contains(i))
                        {
                            continue;
                        }
                        if (i == '[' || i == '{' || i == '$')
                        {
                            skipStack.Push(i);
                        }
                        if (skipStack.Count > 0)
                        {
                            if ((i == ']' && skipStack.Peek() == '[') ||
                                (i == '}' && skipStack.Peek() == '{') ||
                                (i == ' ' && skipStack.Peek() == '$'))
                            {
                                skipStack.Pop();
                                continue;
                            }
                        }
                        // Only add to the stack if we aren't skipping evals and annotations.
                        if (skipStack.Count == 0)
                        {
                            // skip double spaces
                            if (i == ' ' && cleanLine[cleanLine.Length - 1] == ' ')
                                continue;
                            if (lastLastLastChar == ' ' && lastLastChar == '1' && lastChar == '.' && i == ' ')
                            {
                                // Multiple games in 1 line.
                                cleanLine.Remove(cleanLine.Length - 3, 3);
                                linesParse.Add(cleanLine.ToString());
                                cleanLine.Clear();
                                cleanLine.Append("1. ");
                                continue;
                            }
                            lastLastLastChar = lastLastChar;
                            lastLastChar = lastChar;
                            lastChar = i;
                            cleanLine.Append(i);
                        }
                    }
                    if (cleanLine.Length > 3)
                    {
                        linesParse.Add(cleanLine.ToString());
                        cleanLine.Clear();
                    }
                }

                foreach (var pgn in linesParse)
                {
                    pgnGameStack = new Stack<List<string>>();
                    pgnGameStack.Push(new List<string>()); // game 1 sits on the top of the stack
                    string[] movesList = pgn.Split(' ');
                    foreach (var m in movesList)
                    {
                        if ((m.StartsWith("1/2")) ||
                            (m.StartsWith("1-")) ||
                            (m.StartsWith("0-")))
                        {
                            continue; // skip game results
                        }
                        bool isNewSubline = m.StartsWith("(");
                        if (isNewSubline)
                        {
                            // clone the top stack and drop the last move off it
                            List<string> cloneTarget = pgnGameStack.Peek();
                            List<string> clonedGame = new List<string>();
                            for (int i = 0; i < cloneTarget.Count - 1; i++)
                            {
                                clonedGame.Add(cloneTarget[i]);
                            }
                            pgnGameStack.Push(clonedGame);
                        }

                        bool isEndSubline = m.EndsWith(")");

                        var move = m;
                        if (isEndSubline || isNewSubline)
                            move = move.Replace("(", "").Replace(")", "");

                        // Move counters always start with numbers only parse actual moves.
                        if (!Regex.IsMatch(move, @"^[0-9]"))
                        {
                            pgnGameStack.Peek().Add(move);
                        }

                        if (isEndSubline)
                        {
                            // clean up and pop the stack...
                            linesRet.Add(pgnGameStack.Pop());
                        }
                    }
                    linesRet.Add(pgnGameStack.Pop()); // get the base line popped as well...
                }

                return linesRet;
            }
            #endregion
        }
        #endregion
}
