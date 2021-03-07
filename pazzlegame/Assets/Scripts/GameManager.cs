using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ゲーム管理クラス
public class GameManager : MonoBehaviour
{

    // const.
    public const int MachingCount = 3;

    // enum.
    private enum GameState
    {
        Idle,
        PieceMove,
        MatchCheck,
        DeletePiece,
        FillPiece,
    }

    // serialize field.
    [SerializeField]
    private BallController ballController;
   

    // private.
    private GameState currentState;
    private BallGenerator selectedPiece;

    //-------------------------------------------------------
    // MonoBehaviour Function
    //-------------------------------------------------------
    // ゲームの初期化処理
    private void Start()
    {
        ballController.InitializeBoard(6, 5);

        currentState = GameState.Idle;
    }

    // ゲームのメインループ
    private void Update()
    {
        switch (currentState)
        {
            case GameState.Idle:
                Idle();
                break;
            case GameState.PieceMove:
                PieceMove();
                break;
            case GameState.MatchCheck:
                MatchCheck();
                break;
            case GameState.DeletePiece:
                DeletePiece();
                break;
            case GameState.FillPiece:
                FillPiece();
                break;
            default:
                break;
        }
    }

    //-------------------------------------------------------
    // Private Function
    //-------------------------------------------------------
    // プレイヤーの入力を検知し、ピースを選択状態にする
    private void Idle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedPiece = board.GetNearestPiece(Input.mousePosition);
            currentState = GameState.PieceMove;
        }
    }

    // プレイヤーがピースを選択しているときの処理、入力終了を検知したら盤面のチェックの状態に移行する
    private void PieceMove()
    {
        if (Input.GetMouseButton(0))
        {
            var piece = ballController.GetNearestPiece(Input.mousePosition);
            if (piece != selectedPiece)
            {
                ballController.SwitchPiece(selectedPiece, piece);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            currentState = GameState.MatchCheck;
        }
    }

    // 盤面上にマッチングしているピースがあるかどうかを判断する
    private void MatchCheck()
    {
        if (ballController.HasMatch())
        {
            currentState = GameState.DeletePiece;
        }
        else
        {
            currentState = GameState.Idle;
        }
    }

    // マッチングしているピースを削除する
    private void DeletePiece()
    {
        ballController.DeleteMatchPiece();
        currentState = GameState.FillPiece;
    }

    // 盤面上のかけている部分にピースを補充する
    private void FillPiece()
    {
        ballController.FillPiece();
        currentState = GameState.MatchCheck;
    }
}
