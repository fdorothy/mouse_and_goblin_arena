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
     * Uses alpha beta to calculate the best move
     */
    public Move bestMoveAB(Board board, int depth, PieceType playerType) {
        PieceType enemy = getEnemyType(playerType);
        float value = float.NegativeInfinity;
        List<Move> moves = possibleMoves(board, playerType);
        Move bestMove = null;
        foreach (Move move in moves)
        {
            Board child = applyMove(board, move, playerType);
            float score = alphabeta(child, depth - 1, float.NegativeInfinity, float.PositiveInfinity, false, enemy);
            if (score > value) {
                bestMove = move;
                value = score;
            }
        }
        return bestMove;
    }

    /**
     * implementation of alpha-beta pruning minimax algo
     * https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
     */
    public float alphabeta(Board board, int depth, float alpha, float beta, bool maximizingPlayer, PieceType playerType) {
        PieceType enemyType = getEnemyType(playerType);
        if (depth == 0) {
            // return score
            return getScore(board, playerType);
        }
        if (maximizingPlayer) {
            float value = float.NegativeInfinity;
            List<Move> moves = possibleMoves(board, playerType);
            foreach (Move move in moves) {
                Board child = applyMove(board, move, playerType);
                value = Mathf.Max(value, alphabeta(child, depth - 1, alpha, beta, false, enemyType));
                alpha = Mathf.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            return value;
        } else {
            float value = float.PositiveInfinity;
            List<Move> moves = possibleMoves(board, playerType);
            foreach (Move move in moves)
            {
                Board child = applyMove(board, move, playerType);
                value = Mathf.Min(value, alphabeta(child, depth - 1, alpha, beta, true, enemyType));
                beta = Mathf.Min(beta, value);
                if (alpha >= beta)
                    break;
            }
            return value;
        }
    }

    PieceType getEnemyType(PieceType t)
    {
        return t == PieceType.MOUSE ? PieceType.GOBLIN : PieceType.MOUSE;
    }
 
    Board applyMove(Board board, Move move, PieceType type) {
        if (move == null)
            return board;
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
