using System;
using System.Collections;
using System.Collections.Generic;

public enum PieceType {
    MOUSE = 0,
    GOBLIN,
    WALL,
    EMPTY
}

public class Piece {
    public PieceType t;
    public bool king = false;
    public int health = 1;
    public int id;
}

public class Location {
    public int x, y;
    public Location(int x, int y) {
        this.x = x;
        this.y = y;
    }
}

public enum ActionType {
    ATTACK, KILL, MOVE, SUMMON
}

public class Action {
    public ActionType t;
    public Location src, dst;
    public int id;
}

public class Board
{
    public int w = 24;
    public int h = 24;
    public Piece[] board;
    public List<Action> actions;
    public bool recordActions = false;

    public Board(Board b) {
        w = b.w;
        h = b.h;
        board = new Piece[w * h];
        for (int i = 0; i < b.board.Length; i++)
        {
            Piece p = new Piece();
            if (b.board[i] != null)
            {
                p.id = b.board[i].id;
                p.t = b.board[i].t;
                p.king = b.board[i].king;
                p.health = b.board[i].health;
                board[i] = p;
            }
        }
    }

    public Board(int w, int h)
    {
        this.w = w;
        this.h = h;
        board = new Piece[w*h];
    }

    public void setPiece(int x, int y, Piece val) {
        board[x + y * w] = val;
    }

    public Piece getPiece(int x, int y) {
        return board[x + y * w];
    }

    public ArrayList possibleMoves(int x, int y) {
        ArrayList result = new ArrayList();
        int i, j;
        for (i = x + 1; i < w && getPiece(i, y) == null; i++) {
            result.Add(new Location(i, y));
        }
        for (i = x - 1; i >= 0 && getPiece(i, y) == null; i--)
        {
            result.Add(new Location(i, y));
        }
        for (j = y + 1; j < w && getPiece(x, j) == null; j++)
        {
            result.Add(new Location(x, j));
        }
        for (j = y - 1; j >= 0 && getPiece(x, j) == null; j--)
        {
            result.Add(new Location(x, j));
        }
        return result;
    }

    public bool isValidMove(Location src, Location dst) {
        ArrayList moves = possibleMoves(src.x, src.y);
        for (int i = 0; i < moves.Count; i++) {
            Location m = (Location)moves[i];
            if (dst.x == m.x && dst.y == m.y)
                return true;
        }
        return false;
    }

    public void move(Location src, Location dst) {
        Piece p = getPiece(src.x, src.y);
        setPiece(dst.x, dst.y, p);
        setPiece(src.x, src.y, null);
        if (recordActions) {
            recordAction(ActionType.MOVE, p.id, src, dst);
        }
    }

    public void removeKilled(PieceType defender) {
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < h; j++) {
                if (getTypeAt(i, j) == defender) {
                    Piece p = getPiece(i, j);
                    if (p.health <= 0)
                    {
                        setPiece(i, j, null);
                        if (recordActions) {
                            recordAction(ActionType.KILL, p.id, new Location(i, j), null);
                        }
                    }
                }
            }
        }
    }

    public void attack(PieceType attackers)
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (getTypeAt(i, j) == attackers)
                {
                    Location l = getNearbyEnemy(i, j, attackers);
                    if (l != null)
                    {
                        attack(new Location(i, j), l);
                    }
                }
            }
        }
    }

    public void attack(Location src, Location dst) {
        Piece p = getPiece(dst.x, dst.y);
        if (p != null) {
            p.health -= 1;
        }

        if (recordActions)
        {
            recordAction(ActionType.ATTACK, p.id, src, dst);
        }
    }

    public Location getNearbyEnemy(int i, int j, PieceType attackers) {
        if (isEnemy(getTypeAt(i-1, j), attackers)) {
            return new Location(i - 1, j);
        }
        if (isEnemy(getTypeAt(i + 1, j), attackers))
        {
            return new Location(i + 1, j);
        }
        if (isEnemy(getTypeAt(i, j-1), attackers))
        {
            return new Location(i, j-1);
        }
        if (isEnemy(getTypeAt(i, j+1), attackers))
        {
            return new Location(i, j+1);
        }
        return null;
    }

    public PieceType checkVictory() {
        bool hasMouseKing = false;
        bool hasGoblinKing = false;
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < h; j++) {
                Piece p = getPiece(i, j);
                if (p != null)
                {
                    if (p.t == PieceType.MOUSE && p.king && p.health > 0)
                        hasMouseKing = true;
                    if (p.t == PieceType.GOBLIN && p.king && p.health > 0)
                        hasGoblinKing = true;
                }
            }
        }
        if (hasMouseKing && !hasGoblinKing)
            return PieceType.MOUSE;
        if (!hasMouseKing && hasGoblinKing)
            return PieceType.GOBLIN;
        return PieceType.EMPTY;
    }

    public PieceType getTypeAt(int i, int j) {
        if (i < 0 || i >= w || j < 0 || j >= h)
            return PieceType.WALL;
        Piece p = getPiece(i, j);
        return (p == null) ? PieceType.EMPTY : p.t;
    }

    public bool isEnemy(PieceType p1, PieceType p2) {
        return ((p1 == PieceType.MOUSE && p2 == PieceType.GOBLIN) ||
                (p1 == PieceType.GOBLIN && p2 == PieceType.MOUSE));
    }

    public void Log() {
        UnityEngine.Debug.Log("board width / height: " + w + ", " + h);
        for (int x = 0; x < w; x++) {
            string line = "";
            for (int y = 0; y < h; y++) {
                Piece p = getPiece(x, y);
                string output = "  ";
                if (p != null)
                {
                    switch (p.t)
                    {
                        case PieceType.WALL:
                            output = "w  ";
                            break;
                        case PieceType.GOBLIN:
                            output = "g" + p.id;
                            break;
                        case PieceType.MOUSE:
                            output = "m" + p.id;
                            break;
                        default: break;
                    }
                }
                line = line + output + ",";
            }
            UnityEngine.Debug.Log(line);
        }
    }

    public void recordAction(ActionType t, int id, Location src, Location dst) {
        Action a = new Action();
        a.t = t;
        a.id = id;
        a.src = src;
        a.dst = dst;
        if (actions == null)
             actions = new List<Action>();
        actions.Add(a);
    }
}
