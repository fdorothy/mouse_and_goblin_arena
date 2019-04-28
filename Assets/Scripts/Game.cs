using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public Tilemap obstacles;
    public Tilemap mice;
    public Tilemap goblins;
    public Tilemap mouseWizard;
    public Tilemap goblinWizard;
    public Board board;

    public Transform mousePrefab;
    public Transform goblinPrefab;
    public Transform mouseWizardPrefab;
    public Transform goblinWizardPrefab;
    public Transform pathPrefab;

    public Transform selectionIcon;

    public Dictionary<int, Transform> pieces;

    public float moveX;
    public float moveY;

    public Location srcLocation;
    public Location dstLocation;

    public ArrayList possibleMoves;

    protected Transform paths;
    protected Transform piecesTransform;

    void Start()
    {
        paths = transform.Find("paths");
        piecesTransform = transform.Find("pieces");
        LoadFromTiled();
        CreatePieces();
    }

    IEnumerator GameLoop() {
        while (true) {
            //yield return new WaitUntil(() => HasInput());
            //Board newBoard = UpdatePieces(moveX, moveY, PieceType.MOUSE);
        }
    }

    void UpdatePieces() {
        for (int i = 0; i < board.w; i++) {
            for (int j = 0; j < board.h; j++) {
                Piece p = board.getPiece(i, j);
                if (p != null && (p.t == PieceType.MOUSE || p.t == PieceType.GOBLIN))
                    UpdatePiece(pieces[p.id], i, j);
            }
        }
    }

    void UpdatePiece(Transform t, int x, int y) {
        t.position = cellBottomPos(x, y);
    }

    private void Update()
    {
        if (srcLocation == null && Input.GetMouseButtonDown(0))
        {
            Vector3 mp = Input.mousePosition;
            mp = Camera.main.ScreenToWorldPoint(mp);
            Location l = toLocation(mp);
            if (l.x != -1)
            {
                Piece p = board.getPiece(l.x, l.y);
                if (p != null && p.t == PieceType.MOUSE)
                {
                    srcLocation = l;
                    FindPossibleMoves();
                }
            }
        }
        else if (srcLocation != null && Input.GetMouseButtonDown(0))
        {
            Vector3 mp = Input.mousePosition;
            mp = Camera.main.ScreenToWorldPoint(mp);
            Location dst = toLocation(mp);

            if (board.isValidMove(srcLocation, dst))
            {
                pushBoard();
                board.move(srcLocation, dst);
                board.attack(PieceType.MOUSE);
                board.removeKilled(PieceType.GOBLIN);
                //UpdatePieces();
                CreatePieces();
                ClearPaths();
                srcLocation = null;

                // do the goblin stuff
                pushBoard();
                AI ai = new AI();
                Move m = ai.bestMove(board, PieceType.GOBLIN);
                if (m != null)
                {
                    board.move(m.src, m.dst);
                    board.attack(PieceType.GOBLIN);
                    board.removeKilled(PieceType.MOUSE);
                    CreatePieces();
                }
            } else {
                srcLocation = null;
                ClearPaths();
            }
        }
        else if (srcLocation != null && (Input.GetKey(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            srcLocation = null;
            ClearPaths();
        }
        else
        {
            if (Input.GetKey(KeyCode.Escape)) {
                // open the pause menu?
            }
        }
    }

    Board pushBoard() {
        board = new Board(board);
        return board;
    }

    void FindPossibleMoves() {
        ClearPaths();
        possibleMoves = board.possibleMoves(srcLocation.x, srcLocation.y);
        for (int i = 0; i < possibleMoves.Count; i++) {
            Location l = (Location)possibleMoves[i];
            Transform t = Instantiate(pathPrefab, paths);
            t.position = cellMiddlePos(l.x, l.y);
        }
    }

    void ClearPaths() {
        foreach (Transform child in paths.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void LoadFromTiled()
    {
        board = new Board(obstacles.size.x, obstacles.size.y);
        int id = 0;
        for (int i = 0; i < board.w; i++)
        {
            for (int j = 0; j < board.h; j++)
            {
                Vector3Int cell = new Vector3Int(obstacles.cellBounds.xMin + i, obstacles.cellBounds.yMin + j, (int)obstacles.cellBounds.zMin);
                if (obstacles.HasTile(cell))
                {
                    Piece piece = new Piece();
                    piece.t = PieceType.WALL;
                    board.setPiece(i, j, piece);
                }
                else if (mice.HasTile(cell))
                {
                    Piece piece = new Piece();
                    piece.t = PieceType.MOUSE;
                    piece.id = id;
                    board.setPiece(i, j, piece);
                    id++;
                }
                else if (goblins.HasTile(cell))
                {
                    Piece piece = new Piece();
                    piece.t = PieceType.GOBLIN;
                    piece.id = id;
                    board.setPiece(i, j, piece);
                    id++;
                } else if (mouseWizard.HasTile(cell)) {
                    Piece piece = new Piece();
                    piece.t = PieceType.MOUSE;
                    piece.id = id;
                    piece.king = true;
                    piece.health = 10;
                    board.setPiece(i, j, piece);
                    id++;
                }
                else if (goblinWizard.HasTile(cell))
                {
                    Piece piece = new Piece();
                    piece.t = PieceType.GOBLIN;
                    piece.id = id;
                    piece.king = true;
                    piece.health = 10;
                    board.setPiece(i, j, piece);
                    id++;
                }
            }
        }
        mice.gameObject.SetActive(false);
        goblins.gameObject.SetActive(false);
        mouseWizard.gameObject.SetActive(false);
        goblinWizard.gameObject.SetActive(false);
    }

    void ClearPieces()
    {
        foreach (Transform child in piecesTransform.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void CreatePieces()
    {
        ClearPieces();
        pieces = new Dictionary<int, Transform>();
        for (int i = 0; i < board.w; i++)
        {
            for (int j = 0; j < board.h; j++)
            {
                Piece p = board.getPiece(i, j);
                if (p != null)
                {
                    if (p.t == PieceType.MOUSE)
                    {
                        pieces[p.id] = createPiece(p.king ? mouseWizardPrefab : mousePrefab, i, j);
                        pieces[p.id].parent = piecesTransform;
                    }
                    else if (p.t == PieceType.GOBLIN)
                    {
                        pieces[p.id] = createPiece(p.king ? goblinWizardPrefab : goblinPrefab, i, j);
                        pieces[p.id].parent = piecesTransform;
                    }
                }
            }
        }
    }

    public Vector3Int getCell(int i, int j) {
        return new Vector3Int(obstacles.cellBounds.xMin + i, obstacles.cellBounds.yMin + j, (int)obstacles.cellBounds.zMin);
    }

    public Transform createPiece(Transform prefab, int i, int j) {
        Transform t = Instantiate(prefab);
        t.position = cellBottomPos(i, j);
        return t;
    }

    public Vector3 cellBottomPos(int i, int j) {
        Vector3 halfX = new Vector3(obstacles.cellSize.x/2.0f, 0.0f, 0.0f);
        return obstacles.CellToWorld(this.getCell(i, j)) + halfX;
    }

    public Vector3 cellMiddlePos(int i, int j) {
        Vector3 middle = obstacles.cellSize * 0.5f;
        return obstacles.CellToWorld(this.getCell(i, j)) + middle;
    }

    public Location toLocation(Vector3 at) {
        Vector3Int cell = obstacles.WorldToCell(at);
        Vector3Int index =
            new Vector3Int(cell.x - obstacles.cellBounds.xMin,
                              cell.y - obstacles.cellBounds.yMin,
                              cell.z - obstacles.cellBounds.zMin);
        if (index.x >= 0 && index.x < obstacles.size.x &&
            index.y >= 0 && index.y < obstacles.size.y)
            return new Location(index.x, index.y);
        return new Location(-1, -1);
    }

    public int getWizardHealth(PieceType type) {
        if (board == null)
            return -1;
        for (int i = 0; i < board.w; i++) {
            for (int j = 0; j < board.h; j++) {
                Piece p = board.getPiece(i, j);
                if (p != null && p.king && p.t == type) {
                    return p.health;
                }
            }
        }
        return 0;
    }

}
