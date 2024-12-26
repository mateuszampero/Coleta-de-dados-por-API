using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace reademail1
{
    public partial class loadingforms : Form
    {

        private Timer _timer;
        private int _dotCount = 0;
        private ProgressBar progressBar;
        private Label lblLoading;

        public loadingforms()
        {
            InitializeComponent();
            InitializeLoadingForm();
        }

        private void InitializeLoadingForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;  // Sem borda
            this.StartPosition = FormStartPosition.CenterScreen;  // Centralizar na tela
            this.BackColor = System.Drawing.Color.WhiteSmoke;  // Fundo preto
            this.Size = new System.Drawing.Size(400, 150);  // Tamanho do form

            // Criar e configurar o Label
            lblLoading = new Label();
            lblLoading.Name = "lblLoading";
            lblLoading.Text = "Carregando";
            lblLoading.ForeColor = System.Drawing.Color.Black;  // Texto branco
            lblLoading.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);  // Tamanho da fonte
            lblLoading.AutoSize = true;
            lblLoading.Location = new System.Drawing.Point(130, 30);  // Posicionar o texto no centro
            this.Controls.Add(lblLoading);

            // Criar e configurar o ProgressBar
            progressBar = new ProgressBar();
            progressBar.Name = "progressBar";
            progressBar.Location = new System.Drawing.Point(50, 70);  // Posição da barra
            progressBar.Size = new System.Drawing.Size(300, 30);  // Tamanho da barra
            progressBar.Style = ProgressBarStyle.Continuous;  // Para ser contínua, sem os passos
            progressBar.Minimum = 0;  // Mínimo 0
            progressBar.Maximum = 100;  // Máximo 100
            progressBar.Value = 0;  // Inicialmente em 0
            this.Controls.Add(progressBar);

            // Configurar Timer
            _timer = new Timer();
            _timer.Interval = 400;  //atualiza bolinha
            _timer.Tick += (s, e) =>
            {
                _dotCount = (_dotCount + 1) % 4;  // Vai de 0 a 3 para criar o efeito de pontos
                lblLoading.Text = "Carregando" + new string('.', _dotCount);
            };
            _timer.Start();
        }

        public void UpdateProgress(int progress)
        {
            if (progressBar != null && progressBar.Maximum >= progress)
            {
                progressBar.Value = progress;
            }
        }

        private void loadingforms_Load(object sender, EventArgs e)
        {



        }
    }
}
