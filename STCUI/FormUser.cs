using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebSockets;
using System.Windows.Forms;
using System.Windows.Markup;
using Newtonsoft.Json;
using STCUI.Models;

namespace STCUI
{
    public partial class FormUser : Form
    {
        public string accessToken;
        public string login;
        public FormUser(string accessToken, string login)
        {
            InitializeComponent();
            this.accessToken = accessToken;
            this.login = login;

            HttpClient client1 = new HttpClient();
            client1.BaseAddress = new Uri("http://localhost:5000/");
            client1.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response1 = client1.GetAsync($"api/STE/getUserData/{login}").Result;
            User user = JsonConvert.DeserializeObject<User>(response1.Content.ReadAsStringAsync().Result);

            guna2TextBox1.Text = user.Surname;
            guna2TextBox2.Text = user.Name;
            guna2TextBox3.Text = user.Patronymic;
        }
    }
}
