using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Identity.Client;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace reademail1
{

    public partial class Form1 : Form
    {

        private readonly string clientId = "ClienteID Aqui";
        private readonly string tenantId = "TenantID Aqui";
        //Guardo sempre seu clientSecret
        private readonly string[] scopes = new[] { "Mail.Read", "email" };

        public Form1()
        {

            InitializeComponent();

            this.MaximizeBox = false;

        }

   
        private async void button1_Click(object sender, EventArgs e)
        {

            string email = txtEmail.Text;

            try
            {
                // Chama o método de busca de e-mails, que já exibe e fecha o LoadingForm conforme necessário
                string emailData = await BuscarEmailAsync();

                // Atualizar a RichTextBox com os dados carregados
                richTextBox1.Text = emailData;
            }
            catch (Exception ex)
            {
                richTextBox1.Text = $"Erro: {ex.Message}";
            }
            finally
            {
                // Certifique-se de habilitar a interface novamente caso ocorra um erro.
                this.Enabled = true;

            }

        }

        private async Task<string> BuscarEmailAsync()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string emailData = string.Empty;

            try
            {
                var cca = PublicClientApplicationBuilder.Create(clientId)
                    .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                    .WithRedirectUri("http://localhost")
                    .Build();

                // Obtém o token de acesso
                var result = await AcquireTokenAsync(cca);

                var loadingForm = new loadingforms();
                loadingForm.Show();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                    var folders = new[] { "inbox", "sentitems", "deleteditems", "drafts" };
                    int folderCount = folders.Length;
                    int emailsLoaded = 0;

                    foreach (var folder in folders)
                    {
                        string folderData = await GetEmailsFromFolder(httpClient, folder);
                        emailData += folderData;
                        emailsLoaded++;

                        int progress = (emailsLoaded * 100) / folderCount;
                        loadingForm.UpdateProgress(progress);
                    }

                    loadingForm.Close();
                }
            }
            catch (Exception ex)
            {
                emailData = $"Erro: {ex.Message}";
            }

            return emailData;

        }

        private async Task<string> GetEmailsFromFolder(HttpClient httpClient, string folder)
        {
            const int maxRetries = 3;
            int attempt = 0;
            string url = $"https://graph.microsoft.com/v1.0/me/mailFolders/{folder}/messages";
            string allEmails = string.Empty;

            while (attempt < maxRetries)
            {
                try
                {
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        allEmails += FormatEmailDataa(content);

                        // Verifica se há mais e-mails (paginação)
                        var data = JObject.Parse(content);
                        url = data["@odata.nextLink"]?.ToString();
                    }
                    else
                    {
                        // Verifica se a resposta é de "Muitas requisições" (429)
                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            // Limitação de requisições, aguarde o tempo recomendado
                            if (response.Headers.RetryAfter != null)
                            {
                                var retryAfterSeconds = response.Headers.RetryAfter.Delta?.TotalSeconds ?? 10;
                                await Task.Delay((int)retryAfterSeconds * 1000);  // Retry após o tempo sugerido
                            }
                            else
                            {
                                await Task.Delay(1000); // Caso não tenha o cabeçalho Retry-After, aguarde 1 segundo
                            }
                        }
                        else
                        {
                            allEmails += $"Erro ao buscar e-mails da pasta {folder}: {response.StatusCode}\n";
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(url)) break; // Se não houver mais páginas, sai do loop

                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries - 1)
                    {
                        allEmails += $"Erro ao acessar os e-mails (tentativas esgotadas): {ex.Message}\n";
                    }
                    else
                    {
                        await Task.Delay(1000 * (int)Math.Pow(2, attempt)); // Backoff exponencial
                    }
                }

                attempt++;
            }

            return allEmails;
        }

        private string FormatEmailDataa(string rawJson)
        {
            try
            {
                JObject data = JObject.Parse(rawJson);
                string formattedData = "---- Lista de E-mails ----\n\n";

                foreach (var email in data["value"])
                {
                    string subject = email["subject"]?.ToString() ?? "Sem Assunto";
                    string from = email["from"]?["emailAddress"]?["address"]?.ToString() ?? "Desconhecido";
                    string to = string.Join(", ", email["toRecipients"]?.Select(r => r["emailAddress"]?["address"]?.ToString()) ?? new List<string>());
                    string receivedDate = email["receivedDateTime"]?.ToString() ?? "Data desconhecida";
                    string bodyPreview = email["bodyPreview"]?.ToString() ?? "Sem conteúdo";

                    formattedData += $"Assunto: {subject}\n";
                    formattedData += $"De: {from}\n";
                    formattedData += $"Para: {to}\n";
                    formattedData += $"Data de Recebimento: {receivedDate}\n";
                    formattedData += $"Resumo do Conteúdo: {bodyPreview}\n";
                    formattedData += new string('-', 50) + "\n\n";
                }

                return formattedData;
            }
            catch (Exception ex)
            {
                return $"Erro ao formatar os dados: {ex.Message}";
            }

        }

        private async Task<AuthenticationResult> AcquireTokenAsync(IPublicClientApplication cca)
        {

            try
            {
                var result = await cca.AcquireTokenInteractive(scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
                return result;
            }
            catch (MsalException msalEx)
            {
                throw new Exception($"Erro ao adquirir token: {msalEx.Message}", msalEx);
            }

        }

        
        private void button4_Click(object sender, EventArgs e)
        {

            richTextBox1.Clear();

        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {



        }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {

            richTextBox1.WordWrap = false;  // Desabilitar a quebra automática de linha
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;  // Barras de rolagem vertical
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Both;     // Barra de rolagem vertical e horizontal
            //richTextBox1.MaxLength = 1000000; // Exemplo: Limitar a 1.000.000 caracteres
            //richTextBox1.Text = formattedData;  // Adicionar o texto completos


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}








