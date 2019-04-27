using System;

public enum PieceType {
    MOUSE = 0,
    GOBLIN,
    WALL,
    EMPTY
}

public class Piece {
    public PieceType t;
    public int id;
}

public class Board
{
    public int w = 24;
    public int h = 24;
    public Piece[] board;

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
                            UnityEngine.Debug.Log("wall");
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
}
