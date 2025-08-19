using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STCUI.Models;
using Newtonsoft.Json;
using Guna.UI2.WinForms;
using System.Web.UI.WebControls;


namespace STCUI
{
    public partial class Form1 : Form
    {

        //Данные пользователя
        private AuthResponse authResponse = new AuthResponse();

        //Для переноса формы
        bool dragging = false;
        Point dragCursorPoint;
        Point dragFormPoint;

        // Переменные для свёртывания или развёртывания панели или кнопки
        private bool _manualsExpand = false;
        private bool _sidebarExpand = true;

        private Form activeForm = null;

        private void openForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel1.Controls.Add(childForm);
            panel1.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }


        public Form1(string accessToken, string login)
        {
            InitializeComponent();
            openForm(new FormMain());
            authResponse.access_token = accessToken;
            authResponse.login = login;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void iconButton2_Click(object sender, EventArgs e)
        {

        }

        private void manualsTransition_Tick(object sender, EventArgs e)
        {
            if (_manualsExpand == false)
            {
                ListOfManuals.Height += 10;
                if (ListOfManuals.Height >= 340)
                {
                    manualsTransition.Stop();
                    _manualsExpand = true;
                }
            }
            else
            {
                ListOfManuals.Height -= 15;
                if (ListOfManuals.Height <= 45)
                {
                    manualsTransition.Stop();
                    _manualsExpand = false; 
                }
            }
        }

        private void Manuals_Click(object sender, EventArgs e)
        {
            manualsTransition.Start();
        }

        private void sidebarTransition_Tick(object sender, EventArgs e)
        {
            if (_sidebarExpand) 
            {
                sidebar.Width -= 10;
                if (sidebar.Width <= 60)
                {
                    _sidebarExpand = false;
                    sidebarTransition.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= 212)
                {
                    _sidebarExpand = true;
                    sidebarTransition.Stop();

                    pnMain.Width = sidebar.Width;
                    pnCreate.Width = sidebar.Width;
                    ListOfManuals.Width = sidebar.Width;
                    pnUser.Width = sidebar.Width;
                }
            }
        }

        private void btmHam_Click(object sender, EventArgs e)
        {
            sidebarTransition.Start();
        }

        // Открытие форм соответствующих кнопок
        private void iconButton1_Click(object sender, EventArgs e)
        {
            openForm(new FormMain());
        }


        // Перенос с помощьюв верхней панели.
        private void TopPanel_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.Manual;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging) 
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormCreate(authResponse.access_token, authResponse.login)); 
        }

        private void myTemplatesBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormMyTemplates(authResponse.access_token, authResponse.login));
        }

        private void listOfLecturersBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormListOfLecturers());
        }

        private void listOfGroupsBtn_Click(object sender, EventArgs e)
        {           
            openForm(new FormListOfGroups());
        }

        private void listOfCabinetsBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormOfCabinets());
        }

        private void listOfSubjectsBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormListOfSubjects());
        }

        private void userBtn_Click(object sender, EventArgs e)
        {
            openForm(new FormUser(authResponse.access_token, authResponse.login));   
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listOfInstitutes_Click(object sender, EventArgs e)
        {
            openForm(new FormListOfInstitutes());
        }

        private void nightControlBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
