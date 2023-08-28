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
 */

using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq; // no Asparallel()

public class MyBot : IChessBot {
    int turn = 0;
    bool isWhite;
    public Move Think(Board board, Timer timer) {
        isWhite = board.IsWhiteToMove;
        Console.WriteLine("Turn: " + ++turn);
        return minimax(2, board, int.MinValue, int.MaxValue, true, isWhite).myMove;
    }

    public class Score {
        //public Score() => (myMove, myValue) = (Move.NullMove, int.MinValue);
        public Score(Move move, int value) => (myMove, myValue) = (move, value);
        public Move myMove { get; }
        public int myValue { get; }
    }

    public static Score minimax(int depth, Board game, int alpha, int beta, bool isMaximisingPlayer, bool isWhite) {
        //bestScore is best amoung Sibling Nodes
        Score bestScore = new Score(Move.NullMove, int.MinValue);
        foreach (Move move in game.GetLegalMoves()) {
            game.MakeMove(move);
            //((isMaximisingPlayer == isWhite)? 1 : -1)
            Score current = new Score(move, evalAsWhite(game) * ((isWhite)? 1 : -1));
            if (current.myValue >= bestScore.myValue) {
                bestScore = current;
            }
            Console.WriteLine("testing " + move + " Value: " + current.myValue);
            if (depth > 0) {
                Score child = minimax(depth - 1, game, alpha, beta, !isMaximisingPlayer, !isWhite);
                if (child.myValue >= bestScore.myValue) {
                    bestScore = child;
                }
            }
            game.UndoMove(move);
            if (isMaximisingPlayer) {
                //alpha is an aggregate of a line of moves (for player)
                alpha = Math.Max(alpha, bestScore.myValue);
                
            } else {
                //beta is aggregate of line of moves (for opponent)
                beta = Math.Min(beta, bestScore.myValue * -1);
                
            }
            //if (beta <= alpha) {
            //    return bestScore;
            //}
        }
        Console.WriteLine("Best " + bestScore.myMove + " Value: " + bestScore.myValue);
        return bestScore;
    }




    public static int evalAsWhite(Board board) {
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
        return (whiteScore - blackScore);
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
