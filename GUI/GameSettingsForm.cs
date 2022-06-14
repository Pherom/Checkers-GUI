using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Checkers
{
    public partial class GameSettingsForm : Form
    {
        public GameSettingsForm()
        {
            InitializeComponent();
        }

        private void checkBoxPlayer2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPlayer2.Checked == true)
            {
                textBoxPlayer2.Enabled = true;
                textBoxPlayer2.Text = String.Empty;
                textBoxPlayer2.Focus();
            }
            else
            {
                textBoxPlayer2.Enabled = false;
                textBoxPlayer2.Text = "[Computer]";
            }
        }

        private int? getRequestedBoardSize()
        {
            int? res = null;
            foreach (Control control in this.Controls)
            {
                RadioButton radio = control as RadioButton;

                if (radio != null && radio.Checked)
                {
                    if (radio == radioButton6x6)
                    {
                        res = 6;
                    }
                    else if (radio == radioButton8x8)
                    {
                        res = 8;
                    }
                    else if (radio == radioButton10x10)
                    {
                        res = 10;
                    }
                }
            }

            return res;
        }
        private bool validatePlayerName(string i_PlayerName)
        {
            return i_PlayerName != String.Empty;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            int? boardSize = getRequestedBoardSize();
            bool multiplayer = checkBoxPlayer2.Checked;
            if (boardSize != null)
            {
                if (validatePlayerName(textBoxPlayer1.Text))
                {
                    if (multiplayer == true)
                    {
                        if (validatePlayerName(textBoxPlayer2.Text))
                        {
                            // Multiplayer
                            Board board = new Board(boardSize.Value);
                            Player player1 = new Player(textBoxPlayer1.Text, true);
                            Player player2 = new Player(textBoxPlayer2.Text, true);
                            Game game = new Game(board, player1, player2);
                            new GameForm(game).ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Missing player 2 name", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Singleplayer
                        Board board = new Board(boardSize.Value);
                        Player player1 = new Player(textBoxPlayer1.Text, true);
                        Game game = new Game(board, player1);
                        new GameForm(game).ShowDialog();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Missing player 1 name", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Board size unchecked", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
