using Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Checkers
{
    public partial class GameForm : Form
    {
        private Game m_Game;
        private BoardButton[,] m_ButtonMatrix;
        private const char k_Player1RegularPiece = 'O';
        private const char k_Player1KingPiece = 'U';
        private const char k_Player2RegularPiece = 'X';
        private const char k_Player2KingPiece = 'K';
        private const char k_EmptyPiece = ' ';
        private BoardButton m_SelectedBoardButton = null;
        private readonly Timer r_Timer = new Timer();


        public GameForm(Game i_Game)
        {
            m_Game = i_Game;
            InitializeComponent();
            r_Timer.Tick += Timer_Tick;
            r_Timer.Interval = 1000;
            labelPlayer1Name.Text = i_Game.Player1.Name + ":";
            labelPlayer2Name.Text = i_Game.Player2.Name + ":";
            m_ButtonMatrix = new BoardButton[i_Game.Board.Size, i_Game.Board.Size];
            createBoard();
            updateScore();
        }

        private void createBoard()
        {
            bool enableButton = false;
            int initOffsetLeftButton = 10;
            int offsetLeftButton = initOffsetLeftButton;
            int widthButton = (this.ClientSize.Width - (initOffsetLeftButton * 2)) / m_Game.Board.Size;

            int offsetTopButton = 45;
            int heightButton = (this.ClientSize.Height - offsetTopButton - 10) / m_Game.Board.Size;

            for (int row = 0; row < m_Game.Board.Size; row++)
            {
                for (int col = 0; col < m_Game.Board.Size; col++)
                {
                    m_ButtonMatrix[row, col] = new BoardButton(row, col);
                    Button button = m_ButtonMatrix[row, col];
                    button.Width = widthButton;
                    button.Height = heightButton;
                    button.Left = offsetLeftButton;
                    button.Top = offsetTopButton;
                    button.Enabled = enableButton;
                    offsetLeftButton += widthButton;
                    this.Controls.Add(button);

                    button.TabStop = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.TextAlign = ContentAlignment.MiddleCenter;
                    if (enableButton == true)
                    {
                        button.Click += button_Click;
                        button.BackColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = Color.FromArgb(85, 85, 85);
                    }

                    button.Text = getCharValueAtSpecificLocation(m_Game.Board.Content[row, col]).ToString();
                    enableButton = !enableButton;
                }
                enableButton = !enableButton;
                offsetLeftButton = initOffsetLeftButton;
                offsetTopButton += heightButton;
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            BoardButton button = sender as BoardButton;
            
            if (m_Game.Board.Content[button.Row, button.Col].Owner == m_Game.CurrentPlayer)
            {
                if (m_SelectedBoardButton != null)
                {
                    m_SelectedBoardButton.Deselect();
                }

                button.Select();
                m_SelectedBoardButton = button;
            }
            else
            {
                if (m_SelectedBoardButton != null && m_Game.Board.Content[button.Row, button.Col].IsEmpty)
                {
                    PlayerTurn currentPlayerTurn = new PlayerTurn(m_SelectedBoardButton.Row, m_SelectedBoardButton.Col, button.Row, button.Col);
                    if (currentPlayerTurn.IsValidForCurrentPlayer(m_Game) == false)
                    {
                        MessageBox.Show("Invalid move!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        m_SelectedBoardButton.Deselect();
                        m_SelectedBoardButton = null;
                        if (m_Game.CurrentPlayer.IsHuman == true)
                        {
                            m_Game.ExecuteTurn(currentPlayerTurn);
                            checkRematch();
                            updateBoard();
                            updateScore();
                        }

                        if (m_Game.Status == Game.eGameStatus.RUNNING &&
                            m_Game.Gamemode == Game.eGamemode.SINGLEPLAYER && 
                            m_Game.CurrentPlayer == m_Game.Player2)
                        {
                            r_Timer.Start();               
                        }
                    }
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            PlayerTurn currentPlayerTurn = PlayerTurn.GenerateRandomValidTurn(m_Game);
            m_Game.ExecuteTurn(currentPlayerTurn);
            checkRematch();
            updateBoard();
            updateScore();
            (sender as Timer).Stop();
            if (m_Game.RequiredTurns.Count != 0)
            {
                r_Timer.Start();
            }
        }

        private bool checkRematch()
        {
            DialogResult? res = null;
            switch (m_Game.Status)
            {
                case Game.eGameStatus.WON:
                    string winMessage = String.Format("{0} Won!{1}Another Round?", m_Game.GetWinner().Name, Environment.NewLine);
                    res = MessageBox.Show(winMessage, "Damka", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    break;
                case Game.eGameStatus.QUIT:
                    r_Timer.Enabled = false;
                    string quitMessage = String.Format("{0} Won, {1} quit!{2}Another Round?", m_Game.GetOpponent(m_Game.CurrentPlayer).Name, m_Game.CurrentPlayer.Name, Environment.NewLine);
                    res = MessageBox.Show(quitMessage, "Damka", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    r_Timer.Enabled = true;
                    break;
                case Game.eGameStatus.DRAW:
                    string tieMessage = String.Format("Tie!{0}Another Round?", Environment.NewLine);
                    res = MessageBox.Show(tieMessage, "Damka", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    break;
            }

            if (res != null)
            {
                if (res.Value == DialogResult.Yes)
                {
                    m_Game.CheckNewGameRequest(true);
                    r_Timer.Stop();
                    updateBoard();
                    updateScore();
                }
                else if (res.Value == DialogResult.No)
                {
                    Application.Exit();
                }
            }

            return res != null && (res.Value == DialogResult.Yes);
        }

        private void updateScore()
        {
            labelPlayer1Score.Text = string.Format("{0}", m_Game.Player1.Score);
            labelPlayer2Score.Text = string.Format("{0}", m_Game.Player2.Score);
        }

        private char getCharValueAtSpecificLocation(Board.Piece piece)
        {
            char res;
            if (piece.IsEmpty)
            {
                res = k_EmptyPiece;
            }
            else
            {
                if (piece.Owner == m_Game.Player1)
                {
                    res = piece.IsKing == false ? k_Player1RegularPiece : k_Player1KingPiece;
                }
                else
                {
                    res = piece.IsKing == false ? k_Player2RegularPiece : k_Player2KingPiece;
                }
            }

            return res;
        }

        private void updateBoard()
        {

            for (int row = 0; row < m_Game.Board.Size; row++)
            {
                for (int col = 0; col < m_Game.Board.Size; col++)
                {
                    m_ButtonMatrix[row, col].Text = getCharValueAtSpecificLocation(m_Game.Board.Content[row, col]).ToString();
                }
            }
        }

        private class BoardButton : Button {
            private readonly int r_Row;
            private readonly int r_Col;
            public BoardButton(int r_Row, int r_Col)
            {
                this.r_Row = r_Row;
                this.r_Col = r_Col;
            }

            public int Row
            {
                get { return r_Row; }
            }

            public int Col
            {
                get { return r_Col; }
            }

            public void Select()
            {
                this.BackColor = Color.Cyan;
            }

            public void Deselect()
            {
                this.BackColor = Color.White;
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool rematchRequested = false;
            if (e.CloseReason == CloseReason.UserClosing)
            {
                m_Game.ExecuteTurn(new PlayerTurn(true));
                rematchRequested = checkRematch();
                if (rematchRequested == true)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
