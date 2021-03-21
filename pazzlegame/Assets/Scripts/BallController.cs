using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool IsTouch = false;

    RectTransform RectTransform;

    [SerializeField]
    private GameObject piecePrefab;

    // 移動用のVectro2
    public Vector2 CurrentPos;

    // 盤面判定用のVector2(0,0)は一番左上の左端
    public Vector2 BoardPos;

    public bool DestroyFlag = false;

    public Sprite[] sprites = new Sprite[6];

    public enum BallType
    {
        Invalide = -1,
        Water,
        Wind,
        Fire,
        Dark,
        Light,
        Heal,
        Num
    }

    public BallType ThisBallType = BallType.Invalide;


    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();

        SetRandomType();

        StartCoroutine(SetCurrentPos());
    }

    public void SetRandomType()
    {
        var randomType = Random.Range(0, (int)BallType.Num);

        GetComponent<Image>().sprite = sprites[randomType];

        ThisBallType = (BallType)randomType;
    }


    IEnumerator SetCurrentPos()
    {
        yield return new WaitForEndOfFrame();
        CurrentPos = RectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsTouch)
        {
            RectTransform.position = Input.mousePosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsTouch = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsTouch = false;
        this.transform.position = CurrentPos;
    }

    public void SetPos(Vector3 nextPos)
    {
        this.RectTransform.position = nextPos;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTouch)
        {
            var afterPos = CurrentPos;
            collision.GetComponent<BallController>().SetPos(CurrentPos);
            CurrentPos = collision.GetComponent<BallController>().CurrentPos;
            collision.GetComponent<BallController>().CurrentPos = afterPos;

        }
    }

    private BallGenerator[,] ballcontroller;
    private int width;
    private int height;
    private int pieceWidth;
    private int randomSeed;

    public void InitializeBoard(int boardWidth, int boardHeight)
    {
        width = boardWidth;
        height = boardHeight;

        pieceWidth = Screen.width / boardWidth;

        ballcontroller = new BallGenerator[width, height];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                CreatePiece(new Vector2(i, j));
            }
        }
    }

    // 入力されたクリック(タップ)位置から最も近いピースの位置を返す
    public BallGenerator GetNearestPiece(Vector3 input)
    {
        var minDist = float.MaxValue;
        BallGenerator nearestPiece = null;

        // 入力値と盤面のピース位置との距離を計算し、一番距離が短いピースを探す
        foreach (var p in ballcontroller)
        {
            var dist = Vector3.Distance(input, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPiece = p;
            }
        }

        return nearestPiece;
    }

    // 盤面上のピースを交換する
    public void SwitchPiece(BallGenerator p1, BallGenerator p2)
    {
        // 位置を移動する
        var p1Position = p1.transform.position;
        p1.transform.position = p2.transform.position;
        p2.transform.position = p1Position;

        // 盤面データを更新する
        var p1BoardPos = GetPieceBoardPos(p1);
        var p2BoardPos = GetPieceBoardPos(p2);
        ballcontroller[(int)p1BoardPos.x, (int)p1BoardPos.y] = p2;
        ballcontroller[(int)p2BoardPos.x, (int)p2BoardPos.y] = p1;
    }

    // 盤面上にマッチングしているピースがあるかどうかを判断する
    public bool HasMatch()
    {
        foreach (var piece in ballcontroller)
        {
            if (IsMatchPiece(piece))
            {
                return true;
            }
        }
        return false;
    }

    // マッチングしているピースを削除する
    public void DeleteMatchPiece()
    {
        // マッチしているピースの削除フラグを立てる
        foreach (var piece in ballcontroller)
        {
            piece.deleteFlag = IsMatchPiece(piece);
        }

        // 削除フラグが立っているオブジェクトを削除する
        foreach (var piece in ballcontroller)
        {
            if (piece != null && piece.deleteFlag)
            {
                Destroy(piece.gameObject);
            }
        }
    }

    public void FillPiece()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                FillPiece(new Vector2(i, j));
            }
        }
    }

    //-------------------------------------------------------
    // Private Function
    //-------------------------------------------------------
    // 特定の位置にピースを作成する
    
    // 盤面上の位置からピースオブジェクトのワールド座標での位置を返す
    private Vector3 GetPieceWorldPos(Vector2 boardPos)
    {
        return new Vector3(boardPos.x * pieceWidth + (pieceWidth / 2), boardPos.y * pieceWidth + (pieceWidth / 2), 0);
    }

    // ピースが盤面上のどの位置にあるのかを返す
    private Vector2 GetPieceBoardPos(BallGenerator piece)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (ballcontroller[i, j] == piece)
                {
                    return new Vector2(i, j);
                }
            }
        }

        return Vector2.zero;
    }

    // 対象のピースがマッチしているかの判定を行う
    private bool IsMatchPiece(BallGenerator ball)
    {
        // ピースの情報を取得
        var pos = GetPieceBoardPos(ball);
        var kind = ball.GetKind();

        // 縦方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var verticalMatchCount = GetSameKindPieceNum(kind, pos, Vector2.up) + GetSameKindPieceNum(kind, pos, Vector2.down) + 1;

        // 横方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var horizontalMatchCount = GetSameKindPieceNum(kind, pos, Vector2.right) + GetSameKindPieceNum(kind, pos, Vector2.left) + 1;

        return verticalMatchCount >= GameManager.MachingCount || horizontalMatchCount >= GameManager.MachingCount;
    }

    // 対象の方向に引数で指定したの種類のピースがいくつあるかを返す
    private int GetSameKindPieceNum(PieceKind kind, Vector2 piecePos, Vector2 searchDir)
    {
        var count = 0;
        while (true)
        {
            piecePos += searchDir;
            if (IsInBoard(piecePos) && ballcontroller[(int)piecePos.x, (int)piecePos.y].GetKind() == kind)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    // 対象の座標がボードに存在するか(ボードからはみ出していないか)を判定する
    private bool IsInBoard(Vector2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }


    // 特定のピースのが削除されているかを判断し、削除されているなら詰めるか、それができなければ新しく生成する
    private void FillPiece(Vector2 pos)
    {
        var piece = ballcontroller[(int)pos.x, (int)pos.y];
        if (piece != null && !piece.deleteFlag)
        {
            // ピースが削除されていなければ何もしない
            return;
        }
        var checkPos = pos + Vector2.up;
        while (IsInBoard(checkPos))
        {
            var checkPiece = ballcontroller[(int)checkPos.x, (int)checkPos.y];
            if (checkPiece != null && !checkPiece.deleteFlag)
            {
                checkPiece.transform.position = GetPieceWorldPos(pos);
                ballcontroller[(int)pos.x, (int)pos.y] = checkPiece;
                ballcontroller[(int)checkPos.x, (int)checkPos.y] = null;
                return;
            }
            checkPos += Vector2.up;
        }

        // 有効なピースがなければ新しく作る
        CreatePiece(pos);
    }
}

