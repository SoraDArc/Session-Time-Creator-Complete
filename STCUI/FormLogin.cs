using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using STCUI.Models;

namespace STCUI
{
    public partial class FormLogin : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormLogin()
        {
            InitializeComponent();
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            string login = guna2TextBox1.Text;
            string password = guna2TextBox2.Text;
            User user = new User();
            user.Login = login.Trim();
            user.Password = password.Trim();
            var data = JsonConvert.SerializeObject(user);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(data, Encoding.UTF8), "userData");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            HttpResponseMessage response = await client.PostAsync($"api/STE/login", formData);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AuthResponse>(json);

                string token = result.access_token;
                string username = result.login;
                Form1 form1 = new Form1(token, username);
                this.Hide();
                form1.Show();

            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            draggingAddModel = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingAddModel)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPointAddModel));
                this.Location = Point.Add(dragFormPointAddModel, new Size(dif));
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draggingAddModel = true;
            dragCursorPointAddModel = Cursor.Position;
            dragFormPointAddModel = this.Location;
        }
    }
}
