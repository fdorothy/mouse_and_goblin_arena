using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Location src, dst;
    public float rating;
    public bool summon = false;
    public Move(Location src, Location dst) {
        this.src = src;
        this.dst = dst;
    }

    public override string ToString() {
        return "Move " + src.x + "," + src.y + " -> " + dst.x + ", " + dst.y;
    }
}

public class AI
{
    const float FRIEND_PIECE_SCORE = 1.1f;
    const float FOE_PIECE_SCORE = -1.0f;
    const float FRIEND_WIZARD_HEALTH = 2.1f;
    const float FOE_WIZARD_HEALTH = -2.0f;

    /**
     * Finds the best move based on a scoring criteria
     * for the current board
     */
    public Move bestMove(Board board, PieceType type, int maxDepth = 1) {
        PieceType enemyType = (type == PieceType.GOBLIN ? PieceType.MOUSE : PieceType.GOBLIN);
        List<Move> moves = possibleMoves(board, type);

        float bestScore = -10000.0f;
        float myScore = 0.0f;
        Move result = null;
        for (int i = 0; i < moves.Count; i++) {
            Move m = moves[i];
            Board b = applyMove(board, m, type);
            myScore = getScore(b, type) + Random.Range(-0.25f, 0.25f);

            // walk the tree to find the best score
            if (maxDepth > 0)
            {
                // apply the best move for the other player
                Move opponentMove = bestMove(b, enemyType, 0);
                Board b2 = applyMove(b, opponentMove, enemyType);

                Move nextBestMove = bestMove(b2, type, 0);
                myScore = myScore + (nextBestMove.rating - myScore) / 2.0f;
            }

            if (myScore > bestScore) {
                bestScore = myScore;
                result = m;
            }
        }
        result.rating = bestScore;
        return result;
    }

    Board applyMove(Board board, Move move, PieceType type) {
        PieceType enemyType = type == PieceType.MOUSE ? PieceType.GOBLIN : PieceType.MOUSE;
        Board b = new Board(board);
        if (move.summon)
            b.summon(move.src, move.dst);
        else
            b.move(move.src, move.dst);
        b.attack(type);
        b.removeKilled(enemyType);
        return b;
    }

    /**
     * Gets a list of possible moves
     */
    List<Move> possibleMoves(Board b, PieceType piece) {
        List<Move> result = new List<Move>();
        List<Location> pieces = getPieces(b, piece);
        // ???

        for (int i = 0; i < pieces.Count; i++) {
            Location src = pieces[i];
            ArrayList moves = b.possibleMoves(src.x, src.y);
            for (int j = 0; j < moves.Count; j++)
            {
                Location dst = (Location)moves[j];
                result.Add(new Move(src, dst));
            }

            Piece p = b.getPiece(src.x, src.y);
            if (p != null && p.king) {
                for (int j = 0; j < moves.Count; j++)
                {
                    Location dst = (Location)moves[j];
                    Move m = new Move(src, dst);
                    m.summon = true;
                    result.Add(m);
                }
            }
        }
        return result;
    }

    /**
     * Gets a list of pieces that can move
     */
    List<Location> getPieces(Board b, PieceType type)
    {
        List<Location> result = new List<Location>();
        for (int i = 0; i < b.w; i++)
        {
            for (int j = 0; j < b.h; j++)
            {
                if (b.getTypeAt(i, j) == type)
                    result.Add(new Location(i, j));
            }
        }
        return result;
    }

    /**
     * Scoring method for a board
     */
    float getScore(Board b, PieceType type) {
        float score = 0.0f;
        for (int i = 0; i < b.w; i++) {
            for (int j = 0; j < b.h; j++) {
                Piece p = b.getPiece(i, j);
                if (p != null) {
                    if (type == p.t) {
                        // friend
                        if (p.king)
                        {
                            score += p.health * FRIEND_WIZARD_HEALTH;
                        }
                        score += FRIEND_PIECE_SCORE;
                    } else if (b.isEnemy(p.t, type)) {
                        // foe
                        if (p.king) {
                            score += p.health * FOE_WIZARD_HEALTH;
                        }
                        score += FOE_PIECE_SCORE;
                    }
                }
            }
        }
        return score;
    }
}
