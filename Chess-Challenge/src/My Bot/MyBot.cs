/*
https://github.com/SebLague/Chess-Challenge

## Rules
* You may participate alone, or in a group of any size.
* You may submit a maximum of two entries.
  * Please only submit a second entry if it is significantly different from your first bot (not just a minor tweak).
  * Note: you will need to log in with a second Google account if you want submit a second entry.
* Only the following namespaces are allowed:
    * `ChessChallenge.API`
    * `System`
    * `System.Numerics`
    * `System.Collections.Generic`
    * `System.Linq`
      * You may not use the `AsParallel()` function
* As implied by the allowed namespaces, you may not read data from a file or access the internet, nor may you create any new threads or tasks to run code in parallel/in the background.
* You may not use the unsafe keyword.
* You may not store data inside the name of a variable/function/class etc (to be extracted with `nameof()`, `GetType().ToString()`, `Environment.StackTrace` and so on). Thank you to [#12](https://github.com/SebLague/Chess-Challenge/issues/12) and [#24](https://github.com/SebLague/Chess-Challenge/issues/24).
* If your bot makes an illegal move or runs out of time, it will lose the game.
   * Games are played with 1 minute per side by default (this can be changed in the settings class). The final tournament time control is TBD, so your bot should not assume a particular time control, and instead respect the amount of time left on the timer (given in the Think function).
* Your bot may not use more than 256mb of memory for creating look-up tables (such as a transposition table).
* If you have added a constructor to MyBot (for generating look up tables, etc.) it may not take longer than 5 seconds to complete.
* All of your code/data must be contained within the _MyBot.cs_ file.
   * Note: you may create additional scripts for testing/training your bot, but only the _MyBot.cs_ file will be submitted, so it must be able to run without them.
   * You may not rename the _MyBot_ struct or _Think_ function contained in the _MyBot.cs_ file.
   * The code in MyBot.cs may not exceed the _bot brain capacity_ of 1024 (see below).
 2307 */

using ChessChallenge.API;
using System;
using System.Linq; // no Asparallel()


public class MyBot : IChessBot {
    //int maxDepth = 4;
    //Score[][] scores = new Score[250][];
    public Move Think(Board board, Timer timer) {
        //bool color = board.IsWhiteToMove;
        //int alpha = -100000;
        //int beta = 100000;
        //Score think = Ids(maxDepth, board, color, alpha, beta);
        Score think = Negamax(5, board, board.IsWhiteToMove, -100000, 100000);
        //Console.WriteLine("Time: " + timer.MillisecondsElapsedThisTurn);
        return think.myMove;
    }
    /*public Score Ids(int maxDepth, Board board, bool color, int alpha, int beta) {
        Score totalBest = new(Move.NullMove, 2401);
        for (int currentDepth = 0; currentDepth <= maxDepth; currentDepth++) {
            Score currentBest = Negamax(currentDepth, board, color, alpha, beta);
            Console.WriteLine("currentBest Score: " + currentBest.myValue);
            //Console.WriteLine("currentBest " + currentBest.myMove);
            if(currentBest.myValue > totalBest.myValue) {
                totalBest = currentBest;
                return totalBest;
            }
        }
        return totalBest;
    }*/
    Score Negamax(int depth, Board board, bool color, int alpha, int beta) {
        Score defaultScore = new Score(Move.NullMove, Eval(board, color));
        Move[] moves = board.GetLegalMoves(); //getLegalMoves(board, maxDepth-depth);
        int[] moveScores = new int[moves.Length];
        if (depth == 0) {
            return defaultScore;
        }
        Score bestScore = new(Move.NullMove, int.MinValue);
        moves = orderMoves(ref moves, moveScores, color);
        foreach (Move move in moves) {
            board.MakeMove(move);
            Score score = new(move, -Negamax(depth - 1, board, !color, -beta, -alpha).myValue);
            board.UndoMove(move);
            if (score.myValue > bestScore.myValue) {
                bestScore = score;
            }
            if (bestScore.myValue > alpha) {
                bestScore = new Score(move, bestScore.myValue);
                alpha = bestScore.myValue;
            }
            if (alpha >= beta) {
                break;
            }
        }
        return bestScore;
    }
    /*public static Move[] getLegalMoves(Board board, int depthIndex) {
        Move[] moves = board.GetLegalMoves();
        return moves;
    }*/
    int Eval(Board board, bool color) {
        //float endgameT = 0;
        int material = boardEvaluate(board, color);
        int bitBoard = EvaluatePieceSquareTables(color);
        return material + bitBoard;
        int boardEvaluate(Board board, bool color) {
            char[] pieceNames = { 'p', 'n', 'b', 'r', 'q', 'k' };
            int[] pieceValues = { 10, 30, 35, 50, 90, 2000, }; //2400
            string state = board.GetFenString().Replace("/", string.Empty).Split()[0];
            string chars = new(state.Where(c => c != '-' && (c < '0' || c > '9')).ToArray());
            int whiteScore = 0;
            int blackScore = 0;
            for (int i = 0; i < chars.Length; i++) {
                int score = pieceValues[Array.IndexOf(pieceNames, Char.ToLower(chars[i]))];
                if (Char.IsUpper(chars[i])) {
                    whiteScore += score;
                }
                if (Char.IsLower(chars[i])) {
                    blackScore += score;
                }
            }
            int finalScore = (whiteScore - blackScore) * ((color) ? 1 : -1);
            if (board.IsInCheckmate() == true | board.IsInCheck() == true) {
                finalScore += 10000 * ((color) ? 1 : -1);
            }
            return finalScore;
        }
        int EvaluatePieceSquareTables(bool color) {
            int value = EvaluatePieceSquareTable(PieceSquareTable.Rooks, board.GetPieceList(PieceType.Rook, color), color);
            value += EvaluatePieceSquareTable(PieceSquareTable.Knights, board.GetPieceList(PieceType.Knight, color), color);
            value += EvaluatePieceSquareTable(PieceSquareTable.Bishops, board.GetPieceList(PieceType.Bishop, color), color);
            value += EvaluatePieceSquareTable(PieceSquareTable.Queens, board.GetPieceList(PieceType.Queen, color), color);
            //value += EvaluatePieceSquareTable(PieceSquareTable.Pawns, board.GetPieceList(PieceType.Pawn, color), color);
            //int pawnLate = EvaluatePieceSquareTable(PieceSquareTable.PawnsEnd, board.Pawns[colorIndex], color);
            //value += pawnEarly * (1 - endgameT));
            //value += (int)(pawnLate * endgameT);
            value += EvaluatePieceSquareTable(PieceSquareTable.KingStart, board.GetPieceList(PieceType.King, color), color);
            //value += (int)(kingEarlyPhase * (1 - endgameT));
            //int kingLatePhase = PieceSquareTable.Read(PieceSquareTable.KingEnd, board.KingSquare[colourIndex], color);
            //value += (int)(kingLatePhase * (endgameT));
            //None,   // 0
            //Pawn,   // 1
            //Knight, // 2
            //Bishop, // 3
            //Rook,   // 4
            //Queen,  // 5
            //King    // 6
            return value;
        }
    }
    static int EvaluatePieceSquareTable(int[] table, PieceList pieceList, bool color) {
        int value = 0;
        foreach (Piece piece in pieceList) {
            value += table[piece.Square.Index];
        }
        return value;
    }
    /*
    public static void moveEvaluate(Board board, Move move) {
        int score = 0;
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int movePiece = board.Square[startSquare];
        int movePieceType = Piece.PieceType(movePiece);
        int capturePieceType = Piece.PieceType(board.Square[targetSquare]);
        bool isCapture = capturePieceType != Piece.None;
        int pieceValue = GetPieceValue(movePieceType);
        if (isCapture) {
            // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
            int captureMaterialDelta = GetPieceValue(capturePieceType) - pieceValue;
            bool opponentCanRecapture = BitBoardUtility.ContainsSquare(oppPawnAttacks | oppAttacks, targetSquare);
            if (opponentCanRecapture) {
                score += (captureMaterialDelta >= 0 ? winningCaptureBias : losingCaptureBias) + captureMaterialDelta;
            } else {
                score += winningCaptureBias + captureMaterialDelta;
            }
        }

        if (movePieceType == PieceType.isPawn) {
            if (flag == Move.PromoteToQueenFlag && !isCapture) {
                score += promoteBias;
            }
        } else if (movePieceType == Piece.King) {
        } else {
            int toScore = PieceSquareTable.Read(movePiece, targetSquare);
            int fromScore = PieceSquareTable.Read(movePiece, startSquare);
            score += toScore - fromScore;

            if (BitBoardUtility.ContainsSquare(oppPawnAttacks, targetSquare)) {
                score -= 50;
            } else if (BitBoardUtility.ContainsSquare(oppAttacks, targetSquare)) {
                score -= 25;
            }

        }
    
        if (!isCapture) {
            //score += regularBias;
            bool isKiller = !inQSearch && ply < maxKillerMovePly && killerMoves[ply].Match(move);
            score += isKiller ? killerBias : regularBias;
            score += History[board.MoveColourIndex, move.StartSquare, move.TargetSquare];
        }

    }
    */

    static class PieceSquareTable {
        //public static int Read(int[] table, int square, bool isWhite) {
        //    if (isWhite) {
        //        int file = BoardHelper.FileIndex(square);
        //        int rank = BoardHelper.RankIndex(square);
        //        rank = 7 - rank;
        //        square = BoardHelper.IndexFromCoord(file, rank);
        //    }
        //    return table[square];
        //}
        //public static int Read(int piece, int square) {
        //    return Tables[piece][square];
        //}
        /*public static readonly int[] Pawns = {
             0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
             5,  5, 10, 25, 25, 10,  5,  5,
             0,  0,  0, 20, 20,  0,  0,  0,
             5, -5,-10,  0,  0,-10, -5,  5,
             5, 10, 10,-20,-20, 10, 10,  5,
             0,  0,  0,  0,  0,  0,  0,  0,
        };*/
        /*public static readonly int[] PawnsEnd = {
             0,   0,   0,   0,   0,   0,   0,   0,
            80,  80,  80,  80,  80,  80,  80,  80,
            50,  50,  50,  50,  50,  50,  50,  50,
            30,  30,  30,  30,  30,  30,  30,  30,
            20,  20,  20,  20,  20,  20,  20,  20,
            10,  10,  10,  10,  10,  10,  10,  10,
            10,  10,  10,  10,  10,  10,  10,  10,
             0,   0,   0,   0,   0,   0,   0,   0,
        };*/
        public static readonly int[] Rooks =  {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0,
        };
        public static readonly int[] Knights = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };
        public static readonly int[] Bishops = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };
        public static readonly int[] Queens = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20,
        };
        public static readonly int[] KingStart = {
            -80,-70,-70,-70,-70,-70,-70,-80,
            -60,-60,-60,-60,-60,-60,-60,-60,
            -40,-50,-50,-60,-60,-50,-50,-40,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20, -5, -5, -5, -5, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20,
        };
        /*public static readonly int[] KingEnd = {
            -20, -10, -10, -10, -10, -10, -10, -20,
             -5,   0,   5,   5,   5,   5,   0,  -5,
            -10,  -5,  20,  30,  30,  20,  -5, -10,
            -15, -10,  35,  45,  45,  35, -10, -15,
            -20, -15,  30,  40,  40,  30, -15, -20,
            -25, -20,  20,  25,  25,  20, -20, -25,
            -30, -25,   0,   0,   0,   0, -25, -30,
            -50, -30, -30, -30, -30, -30, -30, -50, };*/
        static readonly int[][] Tables;

        // Piece Types
        //const int None = 0;
        //const int Pawn = 1;
        //const int Knight = 2;
        //const int Bishop = 3;
        //const int Rook = 4;
        //const int Queen = 5;
        //const int King = 6;

        // Piece Colours
        //const int White = 0;
        //const int Black = 8;

        // Pieces
        //const int WhitePawn = Pawn | White; // 1
        const int WhiteKnight = 2 | 0; // 2
        const int WhiteBishop = 3 | 0; // 3
        const int WhiteRook = 4 | 0; // 4
        const int WhiteQueen = 5 | 0; // 5
        //const int WhiteKing = King | White; // 6

        //const int BlackPawn = Pawn | Black; // 9
        const int BlackKnight = 2 | 8; // 10
        const int BlackBishop = 3 | 8; // 11
        const int BlackRook = 4 | 8; // 12
        const int BlackQueen = 5 | 8; // 13
        //const int BlackKing = King | Black; // 14

        //const int MaxPieceIndex = BlackKing;
        static PieceSquareTable() {
            Tables = new int[64][];
            //Tables[WhitePawn] = Pawns;
            Tables[WhiteRook] = Rooks;
            Tables[WhiteKnight] = Knights;
            Tables[WhiteBishop] = Bishops;
            Tables[WhiteQueen] = Queens;

            //Tables[BlackPawn] = GetFlippedTable(Pawns);
            Tables[BlackRook] = GetFlippedTable(Rooks);
            Tables[BlackKnight] = GetFlippedTable(Knights);
            Tables[BlackBishop] = GetFlippedTable(Bishops);
            Tables[BlackQueen] = GetFlippedTable(Queens);
        }
        static int[] GetFlippedTable(int[] table) {
            int[] flippedTable = new int[table.Length];

            for (int i = 0; i < table.Length; i++) {
                Coord coord = new Coord(i);
                Coord flippedCoord = new Coord(coord.fileIndex, 7 - coord.rankIndex);
                flippedTable[flippedCoord.SquareIndex] = table[i];
            }
            return flippedTable;
        }
    }
    /*static class BoardHelper {
        //ublic static int RankIndex(int squareIndex) {
        //    return squareIndex >> 3;
        //}
        // File (0 to 7) of square 
        //public static int FileIndex(int squareIndex) {
        //    return squareIndex & 0b000111;
        //}
        //public static int IndexFromCoord(int fileIndex, int rankIndex) {
        //    return rankIndex * 8 + fileIndex;
        //}
        //public static int IndexFromCoord(Coord coord) {
        //   return IndexFromCoord(coord.fileIndex, coord.rankIndex);
        //}
        //static Coord CoordFromIndex(int squareIndex) {
        //    return new Coord(FileIndex(squareIndex), RankIndex(squareIndex));
        //}
        //static bool LightSquare(int fileIndex, int rankIndex) {
        //    return (fileIndex + rankIndex) % 2 != 0;
        //}
        //static bool LightSquare(int squareIndex) {
        //    return LightSquare(FileIndex(squareIndex), RankIndex(squareIndex));
        //}
        //static string SquareNameFromCoordinate(int fileIndex, int rankIndex) {
        //    return fileNames[fileIndex] + "" + (rankIndex + 1);
        //}
        //static string SquareNameFromIndex(int squareIndex) {
        //    return SquareNameFromCoordinate(CoordFromIndex(squareIndex));
        //}
        //static string SquareNameFromCoordinate(Coord coord) {
        //    return SquareNameFromCoordinate(coord.fileIndex, coord.rankIndex);
        //}
        //public static int SquareIndexFromName(string name) {
        //    char fileName = name[0];
        //    char rankName = name[1];
        //    int fileIndex = fileNames.IndexOf(fileName);
        //    int rankIndex = rankNames.IndexOf(rankName);
        //    return IndexFromCoord(fileIndex, rankIndex);
        //}
        //static bool IsValidCoordinate(int x, int y) => x >= 0 && x < 8 && y >= 0 && y < 8;
    }*/
    Move[] orderMoves(ref Move[] moves, int[]moveScores, bool color) { // score all moves, rearrange moves in index
        for (int i = 0; i < moves.Length - 1; i++) { 
            for (int j = i + 1; j > 0; j--) {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j]) {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
        return moves;
    }

    public class Score {
        public Score(Move move, int value) => (myMove, myValue) = (move, value);
        public Move myMove { get; }
        public int myValue { get; }
    }

    public class Coord {
        public int fileIndex;
        public int rankIndex;
        public Coord(int fileIndex, int rankIndex) {
            this.fileIndex = fileIndex;
            this.rankIndex = rankIndex;
        }
        public Coord(int squareIndex) {
            this.fileIndex = squareIndex & 0b000111;
            this.rankIndex = squareIndex >> 3;
        }
        public int SquareIndex => this.rankIndex* 8 + this.fileIndex;
    }
}
