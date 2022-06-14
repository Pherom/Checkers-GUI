using System;
using System.Collections.Generic;

namespace Engine
{
    public class Player
    {
        private const string k_InvalidNameErrorMessage = "Player name must not contain spaces and must be between 1 and 20 characters";
        private readonly string m_Name;
        private int m_Score = 0;
        private readonly bool m_IsHuman;
        private List<Board.Piece> m_Pieces = new List<Board.Piece>();

        public bool IsHuman
        {
            get
            {
                return m_IsHuman;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public int Score
        {
            get
            {
                return m_Score;
            }
        }

        public List<Board.Piece> Pieces
        {
            get
            {
                return m_Pieces;
            }
        }

        public Player(string i_Name, bool i_IsHuman)
        {
            if (!checkNameValidity(i_Name))
            {
                throw new ArgumentException(k_InvalidNameErrorMessage);
            }

            m_IsHuman = i_IsHuman;
            m_Name = i_Name;
        }

        private bool checkNameValidity(string i_Name)
        {
            return i_Name.Length > 0 && i_Name.Length <= 20 && !i_Name.Contains(" ");
        }

        public void InitScoreAndList(int numberOfInitialPiecesToStartWith)
        {
            m_Pieces = new List<Board.Piece>(numberOfInitialPiecesToStartWith);
            InitScore(numberOfInitialPiecesToStartWith);
        }

        public void InitScore(int numberOfInitialPiecesToStartWith)
        {
            m_Score = numberOfInitialPiecesToStartWith;
        }

        public void AddToScore(int numberOfPointsToAdd)
        {
            m_Score += numberOfPointsToAdd;
        }
    }
}
