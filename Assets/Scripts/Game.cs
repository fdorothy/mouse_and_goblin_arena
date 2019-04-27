using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public Tilemap obstacles;
    public Tilemap mice;
    public Tilemap goblins;
    public Board board;

    void Start()
    {
        LoadFromTiled();
    }

    void LoadFromTiled() {
        board = new Board(obstacles.size.x, obstacles.size.y);
        int id = 0;
        for (int i = 0; i < board.w; i++) {
            for (int j = 0; j < board.h; j++) {
                Vector3Int cell = new Vector3Int(obstacles.cellBounds.xMin + i, obstacles.cellBounds.yMin+j, (int)obstacles.cellBounds.zMin);
                if (obstacles.HasTile(cell)) {
                    Piece piece = new Piece();
                    piece.t = PieceType.WALL;
                    board.setPiece(i, j, piece);
                } else if (mice.HasTile(cell)) {
                    Piece piece = new Piece();
                    piece.t = PieceType.MOUSE;
                    piece.id = id;
                    board.setPiece(i, j, piece);
                    id++;
                } else if (goblins.HasTile(cell))
                {
                    Piece piece = new Piece();
                    piece.t = PieceType.GOBLIN;
                    piece.id = id;
                    board.setPiece(i, j, piece);
                    id++;
                }
            }
        }
        mice.gameObject.SetActive(false);
        goblins.gameObject.SetActive(false);
    }
}
