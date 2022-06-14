using System;
using System.Collections.Generic;

namespace Engine
{
    public class PlayerTurn
    {
        private bool m_Quit = false;
        private int m_StartCol;
        private int m_StartRow;
        private int m_EndCol;
        private int m_EndRow;

        public int StartCol
        {
            get
            {
                return m_StartCol;
            }
        }

        public int StartRow
        {
            get
            {
                return m_StartRow;
            }
        }

        public int EndCol
        {
            get
            {
                return m_EndCol;
            }
        }

        public int EndRow
        {
            get
            {
                return m_EndRow;
            }
        }

        public bool Quit
        {
            get
            {
                return m_Quit;
            }
        }

        public PlayerTurn(bool i_Quit)
        {
            m_Quit = i_Quit;
        }

        public PlayerTurn(int i_StartRow, int i_StartCol, int i_EndRow, int i_EndCol)
        {
            m_StartRow = i_StartRow;
            m_StartCol = i_StartCol;
            m_EndRow = i_EndRow;
            m_EndCol = i_EndCol;
        }
        
        private bool isInRange(int i_Size)
        {
            bool notHigherThanSupremum = m_StartRow < i_Size && m_StartCol < i_Size && m_EndRow < i_Size && m_EndCol < i_Size;
            bool notLessThanInfimum = m_StartRow >= 0 && m_StartCol >= 0 && m_EndRow >= 0 && m_EndCol >= 0;
            return notHigherThanSupremum && notLessThanInfimum;
        }

        private bool checkCurrentPlayerPieceIsInStartingPosition(Game i_Game)
        {
            return checkPlayerPieceIsInStartingPosition(i_Game, i_Game.CurrentPlayer);
        }

        private bool checkPlayerPieceIsInStartingPosition(Game i_Game, Player i_Player)
        {
            return i_Game.Board.Content[m_StartRow, m_StartCol].Owner == i_Player;
        }

        private bool checkWhitespaceAtEndPosition(Game i_Game)
        {
            return i_Game.Board.Content[m_EndRow, m_EndCol].IsEmpty;
        }
        // We need this function because List.contains comnpares by reference and not values
        private bool checkIfRequiredListContainsThisTurn(Game i_Game)
        {
            bool isContained = false;
            foreach (PlayerTurn turn in i_Game.RequiredTurns)
            {
                if (m_StartRow == turn.StartRow && m_StartCol == turn.StartCol && m_EndRow == turn.EndRow && m_EndCol == turn.EndCol)
                {
                    isContained = true;
                    break;
                }
            }

            return isContained;
        }

        private bool checkRequiredTurnsContainsThisTurn(Game i_Game)
        {
            return i_Game.RequiredTurns.Count == 0 ? true : checkIfRequiredListContainsThisTurn(i_Game);
        }

        public bool CheckOpponentIsLocatedInPotentialPiece(Game i_Game, Board.Piece i_PotentialOppponentPiece)
        {
            return (i_PotentialOppponentPiece.IsEmpty == false) && (i_Game.CurrentPlayer != i_PotentialOppponentPiece.Owner);
        }


        public Board.Piece GetPotentialPieceJumpedOverIfRegular(Game i_Game)
        {
            Board.Piece res = null;
            int potentialOpponentPieceColValue = (m_EndCol - m_StartCol == 2) ? (m_EndCol - 1) : (m_EndCol + 1);
            int potentialOpponentPieceRowValue = (i_Game.CurrentPlayer == i_Game.Player1) ? (m_EndRow - 1) : (m_EndRow + 1);
            bool notHigherThanSupremum = potentialOpponentPieceColValue < i_Game.Board.Size - 1 && potentialOpponentPieceRowValue < i_Game.Board.Size - 1;
            bool notLessThanInfimum = potentialOpponentPieceColValue > 0 && potentialOpponentPieceRowValue > 0;

            if (notHigherThanSupremum == true && notLessThanInfimum == true)
            {
                res = i_Game.Board.Content[potentialOpponentPieceRowValue, potentialOpponentPieceColValue];
            }

            return res;
        }

        public Board.Piece GetPotentialPieceJumpedOverIfKingAndGoingToNotNativeDirection(Game i_Game)
        {
            Board.Piece res = null;
            int potentialOpponentPieceColValue = (m_EndCol - m_StartCol == 2) ? (m_EndCol - 1) : (m_EndCol + 1);
            int potentialOpponentPieceRowValue = (i_Game.CurrentPlayer == i_Game.Player1) ? (m_EndRow + 1) : (m_EndRow - 1);
            bool notHigherThanSupremum = potentialOpponentPieceColValue < i_Game.Board.Size - 1 && potentialOpponentPieceRowValue < i_Game.Board.Size - 1;
            bool notLessThanInfimum = potentialOpponentPieceColValue > 0 && potentialOpponentPieceRowValue > 0;

            if (notHigherThanSupremum == true && notLessThanInfimum == true)
            {
                res = i_Game.Board.Content[potentialOpponentPieceRowValue, potentialOpponentPieceColValue];
            }

            return res;
        }

        public bool CheckAteOpponent(Game i_Game)
        {
            bool isValid = false;
            int differenceRows = (i_Game.CurrentPlayer == i_Game.Player1) ? (m_EndRow - m_StartRow) : (m_StartRow - m_EndRow);
            differenceRows = (i_Game.Board.Content[m_StartRow, m_StartCol].IsKing) ? Math.Abs(differenceRows) : differenceRows;
            int differenceCols = Math.Abs(m_EndCol - m_StartCol);

            if (differenceCols == 2 && differenceRows == 2)
            {
                Board.Piece jumpedOverThisPieceIfRegular = GetPotentialPieceJumpedOverIfRegular(i_Game);
                Board.Piece jumpedOverThisPieceIfKing = null;

                if (i_Game.Board.Content[m_StartRow, m_StartCol].IsKing == true)
                {
                    jumpedOverThisPieceIfKing = GetPotentialPieceJumpedOverIfKingAndGoingToNotNativeDirection(i_Game);
                }

                isValid = jumpedOverThisPieceIfRegular != null && CheckOpponentIsLocatedInPotentialPiece(i_Game, jumpedOverThisPieceIfRegular) || 
                    (jumpedOverThisPieceIfKing != null && CheckOpponentIsLocatedInPotentialPiece(i_Game, jumpedOverThisPieceIfKing));
            }

            return isValid;
        }

        // Make sure not moving backwards, and if jumping over entity so making sure it's the opponent
        private bool checkDiagonalDirection(Game i_Game) 
        {
            bool isValid = false;
            int differenceRows = (i_Game.CurrentPlayer == i_Game.Player1) ? (m_EndRow - m_StartRow) : (m_StartRow - m_EndRow);
            differenceRows = (i_Game.Board.Content[m_StartRow, m_StartCol].IsKing) ? Math.Abs(differenceRows) : differenceRows;
            int differenceCols = Math.Abs(m_EndCol - m_StartCol);

            // In case moving to whitespace
            if (differenceCols == 1 && differenceRows == 1)
            {
                isValid = true;
            } 
            else
            {
                isValid = CheckAteOpponent(i_Game);
            }

            return isValid;
        }

        public bool IsValidForCurrentPlayer(Game i_Game)
        {
            return isInRange(i_Game.Board.Size) && checkCurrentPlayerPieceIsInStartingPosition(i_Game) &&
                checkWhitespaceAtEndPosition(i_Game) && checkRequiredTurnsContainsThisTurn(i_Game) &&
                checkDiagonalDirection(i_Game);
        }

        public bool IsValidForPlayer(Game i_Game, Player i_Player)
        {
            return isInRange(i_Game.Board.Size) && checkPlayerPieceIsInStartingPosition(i_Game, i_Player) &&
                checkWhitespaceAtEndPosition(i_Game) && checkDiagonalDirection(i_Game);
        }

        public void CheckIfNewEndPosIsCrownAndCrownIfNeeded(Game i_Game)
        {
            int rowToBeCrowned;
            Board.Piece movedToHere = i_Game.Board.Content[m_EndRow, m_EndCol];
            if (movedToHere.IsKing == false)
            {
                rowToBeCrowned = (movedToHere.Owner == i_Game.Player2) ? 0 : (i_Game.Board.Size - 1);
                if (movedToHere.Row == rowToBeCrowned)
                {
                    movedToHere.Crown();
                    movedToHere.Owner.AddToScore(3); // 3 because it's -1 and then +4
                }
            }
        }

        public void UpdatePlayerPiecesListAccordingToTurn(Game i_Game)
        {
            i_Game.CurrentPlayer.Pieces.Remove(i_Game.Board.Content[m_StartRow, m_StartCol]);
            i_Game.CurrentPlayer.Pieces.Add(i_Game.Board.Content[m_EndRow, m_EndCol]);
        }

        public static PlayerTurn GenerateRandomValidTurn(Game i_Game)
        {
            PlayerTurn res = null;
            Player currentPlayer = i_Game.CurrentPlayer;
            List<PlayerTurn> requiredTurns = i_Game.RequiredTurns;
            Random random = new Random();
            Board.Piece chosenPiece;
            List<PlayerTurn> chosenPieceAvailableMoves;

            if (requiredTurns.Count == 0)
            {
                do
                {
                    chosenPiece = currentPlayer.Pieces[random.Next(currentPlayer.Pieces.Count)];
                    chosenPieceAvailableMoves = chosenPiece.GetAvailableMoves(i_Game, currentPlayer);
                } while (chosenPieceAvailableMoves.Count == 0);
                res = chosenPieceAvailableMoves[random.Next(chosenPieceAvailableMoves.Count)];
            }
            else
            {
                res = requiredTurns[random.Next(requiredTurns.Count)];
            }

            return res;
        }
    }
}
