using System;
using System.Collections.Generic;

namespace Engine
{
    public class Game
    {
        public enum eGamemode
        {
            SINGLEPLAYER = 1,
            MULTIPLAYER
        }

        public enum eGameStatus
        {
            RUNNING,
            QUIT,
            WON,
            DRAW
        }

        private const string k_NoDeterminedWinnerErrorMessage = "There is no determined winner yet";
        private readonly eGamemode m_Gamemode;
        private eGameStatus m_Status = eGameStatus.RUNNING;
        private Player m_Player1;
        private Player m_Player2;
        private Player m_CurrPlayer;
        private Board m_Board;
        private List<PlayerTurn> m_RequiredTurns = new List<PlayerTurn>();

        public eGamemode Gamemode
        {
            get
            {
                return m_Gamemode;
            }
        }

        public Player Player1
        {
            get
            {
                return m_Player1;
            }
        }

        public Player Player2
        {
            get
            {
                return m_Player2;
            }
        }

        public Board Board
        {
            get
            {
                return m_Board;
            }
        }

        public Player CurrentPlayer
        {
            get
            {
                return m_CurrPlayer;
            }
        }

        public List<PlayerTurn> RequiredTurns
        {
            get
            {
                return m_RequiredTurns;
            }
        }

        public eGameStatus Status
        {
            get
            {
                return m_Status;
            }
        }

        public Game(Board i_Board, Player i_Player1)
        {
            m_Board = i_Board;
            m_Player1 = i_Player1;
            m_Player1.InitScoreAndList(m_Board.GetInitialPointsPerPlayer());
            m_Player2 = new Player("Computer", false);
            m_Player2.InitScoreAndList(m_Board.GetInitialPointsPerPlayer());
            m_CurrPlayer = m_Player1;
            m_Gamemode = eGamemode.SINGLEPLAYER;
            m_Board.PopulateBoard(m_Player1, m_Player2);
        }

        public Game(Board i_Board, Player i_Player1, Player i_Player2)
        {
            m_Board = i_Board;
            m_Player1 = i_Player1;
            m_Player1.InitScoreAndList(m_Board.GetInitialPointsPerPlayer());
            m_Player2 = i_Player2;
            m_Player2.InitScoreAndList(m_Board.GetInitialPointsPerPlayer());
            m_CurrPlayer = m_Player1;
            m_Gamemode = eGamemode.MULTIPLAYER;
            m_Board.PopulateBoard(m_Player1, m_Player2);
        }

        public void ExecuteTurn(PlayerTurn i_Turn)
        {
            bool ateOpponent;
            if (i_Turn.Quit == true)
            {
                m_Status = eGameStatus.QUIT;
            } 
            else
            {
                ateOpponent = i_Turn.CheckAteOpponent(this);
                // Execute turn
                Board.Content[i_Turn.EndRow, i_Turn.EndCol].CopyPiece(Board.Content[i_Turn.StartRow, i_Turn.StartCol]);
                i_Turn.CheckIfNewEndPosIsCrownAndCrownIfNeeded(this);
                i_Turn.UpdatePlayerPiecesListAccordingToTurn(this);
                Board.Content[i_Turn.StartRow, i_Turn.StartCol].Empty(); // Setting whitespace in startPos

                // check if ate opponent and accordingly update the required turns list
                if (ateOpponent == true)
                {
                    updatePotentialEatingSurroundingsInRequiredTurns(Board.Content[i_Turn.EndRow, i_Turn.EndCol]);
                    DestroyOpponent(i_Turn);
                }
                else
                {
                    m_RequiredTurns.Clear();
                }

                // if the current player doesnt have any required turns to do in his next turn
                if (m_RequiredTurns.Count == 0)
                {
                    switchTurn();
                }

                checkAndUpdateGameStatus();
            }
        }

        private void updatePotentialEatingSurroundingsInRequiredTurns(Board.Piece i_StartPos)
        {
            int potentialRowValue;
            if (i_StartPos.IsKing == true)
            {
                // Add all possible 4 moves for king in case of eating
                for (int row = -2; row <= 2; row += 4)
                {
                    for (int col = -2; col <= 2; col += 4)
                    {
                        RequiredTurns.Add(new PlayerTurn(i_StartPos.Row, i_StartPos.Col, row, col));
                    }
                }
            }
            else
            {
                potentialRowValue = (i_StartPos.Owner == Player1) ? (i_StartPos.Row + 2) : (i_StartPos.Row - 2);
                RequiredTurns.Add(new PlayerTurn(i_StartPos.Row, i_StartPos.Col, potentialRowValue, i_StartPos.Col - 2));
                RequiredTurns.Add(new PlayerTurn(i_StartPos.Row, i_StartPos.Col, potentialRowValue, i_StartPos.Col + 2));
            }

            // Remove the turns that shouldn't be in the required turns list
            for (int i = RequiredTurns.Count - 1; i >= 0; i--)
            {
                // If invalid move or didnt ate opponent
                if (RequiredTurns[i].IsValidForCurrentPlayer(this) == false || RequiredTurns[i].CheckAteOpponent(this) == false)
                {
                    RequiredTurns.RemoveAt(i);
                }
            }
        }


        private void updatePointsAndEmptyEatenOpponent(Board.Piece i_EatenPiece)
        {
            i_EatenPiece.Owner.AddToScore(-1);
            GetOpponent(i_EatenPiece.Owner).AddToScore(i_EatenPiece.IsKing == true ? 4 : 1);
            i_EatenPiece.Empty();
        }

        private void DestroyOpponent(PlayerTurn i_Turn)
        {
            Board.Piece potentialMove = i_Turn.GetPotentialPieceJumpedOverIfRegular(this);
            if (potentialMove != null && i_Turn.CheckOpponentIsLocatedInPotentialPiece(this, potentialMove))
            {
                updatePointsAndEmptyEatenOpponent(potentialMove);
            }
            else if (Board.Content[i_Turn.EndRow, i_Turn.EndCol].IsKing == true)
            {
                potentialMove = i_Turn.GetPotentialPieceJumpedOverIfKingAndGoingToNotNativeDirection(this);
                if (i_Turn.CheckOpponentIsLocatedInPotentialPiece(this, potentialMove))
                {
                    updatePointsAndEmptyEatenOpponent(potentialMove);
                }
            }
        }

        private void switchTurn()
        {
            m_CurrPlayer = GetOpponent(m_CurrPlayer);
        }

        public Player GetOpponent(Player i_Player)
        {
            return i_Player == m_Player1 ? m_Player2 : m_Player1;
        }

        public Player GetWinner()
        {
            Player winner = null;
            if (Player1.Pieces.Count == 0 || !hasAvailableMoves(Player1))
            {
                winner = Player2;
            }
            else if (Player2.Pieces.Count == 0 || !hasAvailableMoves(Player2))
            {
                winner = Player1;
            }

            return winner;
        }

        private bool hasAvailableMoves(Player player)
        {
            bool has = false;

            foreach (Board.Piece piece in player.Pieces)
            {
                if (piece.GetAvailableMoves(this, player).Count > 0)
                {
                    has = true;
                    break;
                }
            }

            return has;
        }

        private void checkAndUpdateGameStatus()
        {
            Player winner = GetWinner();
            if (winner != null)
            {
                m_Status = eGameStatus.WON;
            }
            else if (checkDraw())
            {
                m_Status = eGameStatus.DRAW;
            }
        }
        private bool checkDraw()
        {
            return !hasAvailableMoves(Player1) && !hasAvailableMoves(Player2);
        }

        public void CheckNewGameRequest(bool i_RequestedNewGame)
        {
            if (i_RequestedNewGame == true)
            {
                m_Status = eGameStatus.RUNNING;
                m_CurrPlayer = Player1;
                m_Player1.InitScore(m_Board.GetInitialPointsPerPlayer());
                m_Player2.InitScore(m_Board.GetInitialPointsPerPlayer());   
                Board.ResetBoard(Player1, Player2);
                Board.PopulateBoard(Player1, Player2);
            }
        }

    }
}
