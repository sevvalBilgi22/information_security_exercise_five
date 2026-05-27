using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace exercise_fivee
{
    public partial class Form1 : Form
    {
        // Cryptography properties
        private static readonly byte[] AES_Salt = Encoding.ASCII.GetBytes("OdevTuzu123!");
        private static readonly byte[] AES_IV = new byte[16] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
        private string currentUser = "";
        private readonly string UsersFile = "users.txt";
        private string CurrentUserFile => currentUser + "_data.txt";
        private readonly List<PasswordEntry> passwordList = new List<PasswordEntry>();
        private string hiddenDecryptedPassword = "";

        // UI Components
        private TabControl tabControl;
        private TabPage tabAuth, tabMain;
        private TextBox txtUsername, txtPassword, txtTitle, txtAppPassword, txtUrl, txtNotes, txtSearch;
        private ListBox lstEntries;
        private Label lblSelectedDetails;

        public Form1()
        {
            InitializeComponent();
            BuildCenteredUI(); // Dynamically creates the neat and centered layout
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void BuildCenteredUI()
        {
            // Main Tab Control
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabAuth = new TabPage("Login / Register") { BackColor = Color.WhiteSmoke };
            tabMain = new TabPage("Password Dashboard") { BackColor = Color.WhiteSmoke };

            // ==========================================
            // 1. LOGIN / REGISTER PANEL (CENTERED)
            // ==========================================
            Panel centerAuthPanel = new Panel { Size = new Size(320, 160), Location = new Point(160, 120) };

            Label lblUser = new Label { Text = "Username:", Location = new Point(10, 15), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(130, 12), Width = 160 };

            Label lblMasterPass = new Label { Text = "Master Password:", Location = new Point(10, 55), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(130, 52), Width = 160, PasswordChar = '*' };

            Button btnLogin = new Button { Text = "Login", Location = new Point(130, 95), Width = 75, Height = 30 };
            btnLogin.Click += BtnLogin_Click;

            Button btnRegister = new Button { Text = "Register", Location = new Point(215, 95), Width = 75, Height = 30 };
            btnRegister.Click += BtnRegister_Click;

            centerAuthPanel.Controls.AddRange(new Control[] { lblUser, txtUsername, lblMasterPass, txtPassword, btnLogin, btnRegister });
            tabAuth.Controls.Add(centerAuthPanel);

            // ==========================================
            // 2. PASSWORD DASHBOARD (WELL ORGANIZED)
            // ==========================================
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // Left Side: Search and List
            FlowLayoutPanel leftLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            leftLayout.Controls.Add(new Label { Text = "Search by Title:", AutoSize = true, Margin = new Padding(0, 0, 0, 5) });
            txtSearch = new TextBox { Width = 190 };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            leftLayout.Controls.Add(txtSearch);

            lstEntries = new ListBox { Width = 190, Height = 280, Margin = new Padding(0, 10, 0, 10) };
            lstEntries.SelectedIndexChanged += LstEntries_SelectedIndexChanged;
            leftLayout.Controls.Add(lstEntries);

            Button btnLogout = new Button { Text = "Secure Logout", Width = 190, Height = 30 };
            btnLogout.Click += BtnLogout_Click;
            leftLayout.Controls.Add(btnLogout);

            // Right Side: Details and Actions
            FlowLayoutPanel rightLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };

            rightLayout.Controls.Add(new Label { Text = "Title:", AutoSize = true });
            txtTitle = new TextBox { Width = 360, Margin = new Padding(0, 0, 0, 10) };
            rightLayout.Controls.Add(txtTitle);

            rightLayout.Controls.Add(new Label { Text = "Password:", AutoSize = true });
            FlowLayoutPanel passRow = new FlowLayoutPanel { Width = 365, Height = 30, Margin = new Padding(0, 0, 0, 10) };
            txtAppPassword = new TextBox { Width = 275 };
            Button btnGen = new Button { Text = "Generate", Width = 75, Height = 23 };
            btnGen.Click += BtnGenerate_Click;
            passRow.Controls.AddRange(new Control[] { txtAppPassword, btnGen });
            rightLayout.Controls.Add(passRow);

            rightLayout.Controls.Add(new Label { Text = "URL / App Name:", AutoSize = true });
            txtUrl = new TextBox { Width = 360, Margin = new Padding(0, 0, 0, 10) };
            rightLayout.Controls.Add(txtUrl);

            rightLayout.Controls.Add(new Label { Text = "Additional Notes:", AutoSize = true });
            txtNotes = new TextBox { Width = 360, Margin = new Padding(0, 0, 0, 15) };
            rightLayout.Controls.Add(txtNotes);

            // CRUD Operation Buttons Row
            FlowLayoutPanel crudRow = new FlowLayoutPanel { Width = 360, Height = 35 };
            Button btnAdd = new Button { Text = "Add", Width = 80, Height = 28 }; btnAdd.Click += BtnAdd_Click;
            Button btnUpdate = new Button { Text = "Update", Width = 80, Height = 28 }; btnUpdate.Click += BtnUpdate_Click;
            Button btnDelete = new Button { Text = "Delete", Width = 80, Height = 28 }; btnDelete.Click += BtnDelete_Click;
            crudRow.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });
            rightLayout.Controls.Add(crudRow);

            // Details Dynamic Box
            lblSelectedDetails = new Label { Text = "Details:", Width = 360, Height = 90, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5), Margin = new Padding(0, 5, 0, 10), BackColor = Color.White };
            rightLayout.Controls.Add(lblSelectedDetails);

            // Secure View Row
            FlowLayoutPanel secureRow = new FlowLayoutPanel { Width = 360, Height = 35 };
            Button btnReveal = new Button { Text = "Reveal Password", Width = 120, Height = 28 }; btnReveal.Click += BtnReveal_Click;
            Button btnCopy = new Button { Text = "Copy to Clipboard", Width = 120, Height = 28 }; btnCopy.Click += BtnCopy_Click;
            secureRow.Controls.AddRange(new Control[] { btnReveal, btnCopy });
            rightLayout.Controls.Add(secureRow);

            mainLayout.Controls.Add(leftLayout, 0, 0);
            mainLayout.Controls.Add(rightLayout, 1, 0);
            tabMain.Controls.Add(mainLayout);

            tabControl.TabPages.AddRange(new TabPage[] { tabAuth, tabMain });
            this.Controls.Add(tabControl);
        }

        #region Cryptography Engine

        private string EncryptAES(string plainText, string masterKey)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(masterKey, AES_Salt, 1000);
                aes.Key = keyDerivation.GetBytes(32);
                aes.IV = AES_IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string DecryptAES(string cipherText, string masterKey)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            try
            {
                using (Aes aes = Aes.Create())
                {
                    Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(masterKey, AES_Salt, 1000);
                    aes.Key = keyDerivation.GetBytes(32);
                    aes.IV = AES_IV;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            byte[] cipherBytes = Convert.FromBase64String(cipherText);
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                return "[Error: Decryption Failed / Invalid Key]";
            }
        }

        private string HashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        #endregion

        #region File Handling & Logic

        private void LoadAndDecryptUserData(string masterPassword)
        {
            passwordList.Clear();
            if (!File.Exists(CurrentUserFile))
            {
                File.WriteAllText(CurrentUserFile, "");
                return;
            }

            string fileContent = File.ReadAllText(CurrentUserFile);
            if (string.IsNullOrEmpty(fileContent)) return;

            string decryptedContent = DecryptAES(fileContent, masterPassword);
            string[] lines = decryptedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 4)
                {
                    passwordList.Add(new PasswordEntry { Title = parts[0], EncryptedPassword = parts[1], Url = parts[2], Notes = parts[3] });
                }
            }
            UpdateListBox();
        }

        private void SaveAndEncryptUserData()
        {
            if (string.IsNullOrEmpty(currentUser)) return;

            StringBuilder sb = new StringBuilder();
            foreach (var entry in passwordList)
            {
                sb.AppendLine(entry.Title + "," + entry.EncryptedPassword + "," + entry.Url + "," + entry.Notes);
            }

            string encryptedContent = EncryptAES(sb.ToString(), txtPassword.Text);
            File.WriteAllText(CurrentUserFile, encryptedContent);
        }

        private void UpdateListBox(string filter = "")
        {
            lstEntries.Items.Clear();
            var filtered = passwordList.Where(p => p.Title.ToLower().Contains(filter.ToLower())).ToList();
            foreach (var item in filtered) lstEntries.Items.Add(item.Title);
        }

        #endregion

        #region UI Event Handlers

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;

            if (user == "" || pass == "") { MessageBox.Show("Please fill in all fields!"); return; }

            string hashedPass = HashSHA256(pass);
            string line = user + ":" + hashedPass + Environment.NewLine;

            if (File.Exists(UsersFile) && File.ReadAllLines(UsersFile).Any(l => l.StartsWith(user + ":")))
            {
                MessageBox.Show("User already exists!");
                return;
            }

            File.AppendAllText(UsersFile, line);
            MessageBox.Show("Registration successful! You can now log in.");
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;

            if (!File.Exists(UsersFile)) { MessageBox.Show("User database not found!"); return; }

            string hashedPass = HashSHA256(pass);
            bool success = File.ReadAllLines(UsersFile).Any(l => l == user + ":" + hashedPass);

            if (success)
            {
                currentUser = user;
                LoadAndDecryptUserData(pass);
                tabControl.SelectedTab = tabMain;
                MessageBox.Show("Welcome, " + user + "! Data decrypted.", "Success");
            }
            else
            {
                MessageBox.Show("Invalid credentials!", "Error");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (txtTitle.Text == "" || txtAppPassword.Text == "") { MessageBox.Show("Fields are mandatory!"); return; }

            string encryptedPass = EncryptAES(txtAppPassword.Text, txtPassword.Text);
            passwordList.Add(new PasswordEntry
            {
                Title = txtTitle.Text.Replace(",", ""),
                EncryptedPassword = encryptedPass,
                Url = txtUrl.Text.Replace(",", ""),
                Notes = txtNotes.Text.Replace(",", "")
            });

            UpdateListBox();
            txtTitle.Clear(); txtAppPassword.Clear(); txtUrl.Clear(); txtNotes.Clear();
            MessageBox.Show("Entry added successfully!");
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateListBox(txtSearch.Text);
        }

        private void LstEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstEntries.SelectedIndex == -1) return;
            var entry = passwordList.FirstOrDefault(p => p.Title == lstEntries.SelectedItem.ToString());

            if (entry != null)
            {
                hiddenDecryptedPassword = DecryptAES(entry.EncryptedPassword, txtPassword.Text);
                lblSelectedDetails.Text = "Title: " + entry.Title + "\nURL: " + entry.Url + "\nNotes: " + entry.Notes + "\nPassword: *****";
            }
        }

        private void BtnReveal_Click(object sender, EventArgs e)
        {
            if (lstEntries.SelectedIndex == -1) return;
            MessageBox.Show("Password: " + hiddenDecryptedPassword, "Secure Reveal");
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hiddenDecryptedPassword)) return;
            Clipboard.SetText(hiddenDecryptedPassword);
            MessageBox.Show("Copied to clipboard!");
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[4];
                while (res.Length < 12)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            txtAppPassword.Text = res.ToString();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (lstEntries.SelectedIndex == -1) return;
            var entry = passwordList.FirstOrDefault(p => p.Title == lstEntries.SelectedItem.ToString());

            if (entry != null)
            {
                entry.EncryptedPassword = EncryptAES(txtAppPassword.Text, txtPassword.Text);
                entry.Url = txtUrl.Text;
                entry.Notes = txtNotes.Text;
                MessageBox.Show("Updated successfully!");
                UpdateListBox();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstEntries.SelectedIndex == -1) return;
            var entry = passwordList.FirstOrDefault(p => p.Title == lstEntries.SelectedItem.ToString());

            if (entry != null)
            {
                passwordList.Remove(entry);
                UpdateListBox();
                lblSelectedDetails.Text = "Details:";
                hiddenDecryptedPassword = "";
                MessageBox.Show("Deleted successfully!");
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            SaveAndEncryptUserData();
            currentUser = "";
            passwordList.Clear();
            lstEntries.Items.Clear();
            txtPassword.Clear();
            tabControl.SelectedTab = tabAuth;
            MessageBox.Show("Logged out safely. File encrypted.");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAndEncryptUserData();
        }

        #endregion
    }

    public class PasswordEntry
    {
        public string Title { get; set; }
        public string EncryptedPassword { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
    }
}