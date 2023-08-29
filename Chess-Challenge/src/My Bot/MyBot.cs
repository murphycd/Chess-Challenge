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
    public Move Think(Board board, Timer timer) {
        int depth = 1;
        bool color = board.IsWhiteToMove;
        Console.WriteLine("Turn: " + ++turn);
        //Move[] moves = board.GetLegalMoves();
        Score think = negaMax(depth, board, color);
        Console.WriteLine("Score: " + think.myValue);
        return think.myMove;
    }

    public Score negaMax(int depth, Board board, bool color) {
        Console.WriteLine("depth: " + depth);
        if (depth == 0) {
            Console.WriteLine("Eval: " + evaluate(board, color));
            return new Score(Move.NullMove, evaluate(board, color));
        }
        //Console.WriteLine("depth: " + depth);
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0) {
            Console.WriteLine("Game has ended");
            return new Score(Move.NullMove, evaluate(board, color));
        }
        Score max = new (Move.NullMove, int.MinValue);
        foreach (Move move in moves) {
            Score score = new (move, negaMax(depth - 1, board, !color).myValue);
            if (score.myValue > max.myValue)
                max = score;
        }
        //Console.WriteLine("Max Move: " + max.myMove.ToString());
        //Console.WriteLine("Max Score: " + max.myValue);
        return max;
    }

    public static int evaluate(Board board, bool color) {
        char[] pieceNames = { 'p', 'n', 'b', 'r', 'q', 'k' };
        int[] pieceValues = { 10, 30, 30, 50, 90, 2000, };
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
        Move[] moves = board.GetLegalMoves();
        int mobility = moves.Length * 1;
        return (whiteScore - blackScore) + mobility * ((color) ? -1 : 1);
    }

    public class Score {f
        public Score(Move move, int value) => (myMove, myValue) = (move, value);
        public Move myMove { get; }
        public int myValue { get; }
    }
}
