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
   * blah blah blah
 */

using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq; // no Asparallel()

public class MyBot : IChessBot {
    int turn = 0;
    public Move Think(Board board, Timer timer) {
        Console.WriteLine("Turn: " + ++turn);
        return minimaxRoot(3, board, int.MinValue, int.MaxValue, true);
    }

    public class Score {
        public Score(Move move, int value) => (myMove, myValue) = (move, value);
        public Move myMove { get; }
        public int myValue { get; }
    }

    public static Move minimaxRoot(int depth, Board game, int alpha, int beta, bool isMaximisingPlayer) {
        Move[] newGameMoves = game.GetLegalMoves();
        int bestMoveEval = int.MinValue;
        Move bestMoveFound = Move.NullMove;
        foreach (Move move in newGameMoves) {
            Move newGameMove = move;
            game.MakeMove(newGameMove);
            Score score = minimax(depth - 1, game, alpha, beta, !isMaximisingPlayer);
            game.UndoMove(newGameMove);
            if (score.myValue >= bestMoveEval) {
                bestMoveEval = score.myValue;
                bestMoveFound = newGameMove;
            }
        }
        return bestMoveFound;
    }

    public static Score minimax(int depth, Board game, int alpha, int beta, bool isMaximisingPlayer) {
        if (depth == 0) {
            return new Score(Move.NullMove, Eval(game));
        }
        Move[] newGameMoves = game.GetLegalMoves();
        if (isMaximisingPlayer) {
            Score bestScore = new Score(Move.NullMove, int.MinValue);
            foreach (Move move in newGameMoves) {
                game.MakeMove(move);
                Score test = minimax(depth - 1, game, alpha, beta, !isMaximisingPlayer);
                if (bestScore.myValue <= test.myValue) {
                    bestScore = test;
                }
                game.UndoMove(move);
                alpha = Math.Max(alpha, bestScore.myValue);
                if (beta <= alpha) {
                    return bestScore;
                }
            }
            return bestScore;
        } else {
            Score bestScore = new Score(Move.NullMove, int.MinValue);
            foreach (Move move in newGameMoves) {
                game.MakeMove(move);
                Score test = minimax(depth - 1, game, alpha, beta, !isMaximisingPlayer);
                game.UndoMove(move);
                beta = Math.Min(beta, bestScore.myValue);
                if (beta <= alpha) {
                    return bestScore;
                }
            }
            return bestScore;
        }
    }


    public static int Eval(Board board) {
        char[] pieceNames = { 'p', 'n', 'b', 'r', 'q', 'k' };
        int[] pieceValues = { 10, 30, 30, 50, 90, 900, };
        string state = board.GetFenString().Replace("/", string.Empty).Split()[0];
        string chars = new(state.Where(c => c != '-' && (c < '0' || c > '9')).ToArray());
        int whiteScore = 0;
        int blackScore = 0;
        for (int i = 0; i < chars.Length; i++) {
            int score = pieceValues[Array.IndexOf(pieceNames, Char.ToLower(chars[i]))];
            if (Char.IsUpper(chars[i])) {
                whiteScore = score + whiteScore;
            }
            if (Char.IsLower(chars[i])) {
                blackScore = score + blackScore;
            }
        }
        return (whiteScore - blackScore) * ((board.IsWhiteToMove) ? -1 : 1);
    }
}

/* var reverseArray = function(array) {
     return array.slice().reverse();
 }

 var pawnEvalWhite =
 [
     [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
     [5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0],
     [1.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 1.0],
     [0.5, 0.5, 1.0, 2.5, 2.5, 1.0, 0.5, 0.5],
     [0.0, 0.0, 0.0, 2.0, 2.0, 0.0, 0.0, 0.0],
     [0.5, -0.5, -1.0, 0.0, 0.0, -1.0, -0.5, 0.5],
     [0.5, 1.0, 1.0, -2.0, -2.0, 1.0, 1.0, 0.5],
     [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
 ];

var pawnEvalBlack = reverseArray(pawnEvalWhite);

var knightEval =
 [
     [-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0],
     [-4.0, -2.0, 0.0, 0.0, 0.0, 0.0, -2.0, -4.0],
     [-3.0, 0.0, 1.0, 1.5, 1.5, 1.0, 0.0, -3.0],
     [-3.0, 0.5, 1.5, 2.0, 2.0, 1.5, 0.5, -3.0],
     [-3.0, 0.0, 1.5, 2.0, 2.0, 1.5, 0.0, -3.0],
     [-3.0, 0.5, 1.0, 1.5, 1.5, 1.0, 0.5, -3.0],
     [-4.0, -2.0, 0.0, 0.5, 0.5, 0.0, -2.0, -4.0],
     [-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0]
 ];

var bishopEvalWhite = [
 [-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0],
 [-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0],
 [-1.0, 0.0, 0.5, 1.0, 1.0, 0.5, 0.0, -1.0],
 [-1.0, 0.5, 0.5, 1.0, 1.0, 0.5, 0.5, -1.0],
 [-1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, -1.0],
 [-1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, -1.0],
 [-1.0, 0.5, 0.0, 0.0, 0.0, 0.0, 0.5, -1.0],
 [-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0]
];

var bishopEvalBlack = reverseArray(bishopEvalWhite);

var rookEvalWhite = [
 [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
 [0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5],
 [-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5],
 [-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5],
 [-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5],
 [-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5],
 [-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5],
 [0.0, 0.0, 0.0, 0.5, 0.5, 0.0, 0.0, 0.0]
];

var rookEvalBlack = reverseArray(rookEvalWhite);

var evalQueen =
 [
 [-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0],
 [-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0],
 [-1.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0],
 [-0.5, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5],
 [0.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5],
 [-1.0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0],
 [-1.0, 0.0, 0.5, 0.0, 0.0, 0.0, 0.0, -1.0],
 [-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0]
];

var kingEvalWhite = [

 [-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0],
 [-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0],
 [-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0],
 [-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0],
 [-2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0],
 [-1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0],
 [2.0, 2.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2.0],
 [2.0, 3.0, 1.0, 0.0, 0.0, 1.0, 3.0, 2.0]
];

var kingEvalBlack = reverseArray(kingEvalWhite);




var getPieceValue = function(piece, x, y) {
 if (piece === null) {
 return 0;
}
var getAbsoluteValue = function(piece, isWhite, x, y) {
     if (piece.type === 'p') {
 return 10 + (isWhite ? pawnEvalWhite[y][x] : pawnEvalBlack[y][x]);
} else if (piece.type === 'r')
{
 return 50 + (isWhite ? rookEvalWhite[y][x] : rookEvalBlack[y][x]);
}
else if (piece.type === 'n')
{
 return 30 + knightEval[y][x];
}
else if (piece.type === 'b')
{
 return 30 + (isWhite ? bishopEvalWhite[y][x] : bishopEvalBlack[y][x]);
}
else if (piece.type === 'q')
{
 return 90 + evalQueen[y][x];
}
else if (piece.type === 'k')
{
 return 900 + (isWhite ? kingEvalWhite[y][x] : kingEvalBlack[y][x]);
}
throw "Unknown piece type: " + piece.type;
 };

var absoluteValue = getAbsoluteValue(piece, piece.color === 'w', x, y);
return piece.color === 'w' ? absoluteValue : -absoluteValue;
};*/
