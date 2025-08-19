using Newtonsoft.Json;
using STCUI.Models;
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

namespace STCUI
{
    public partial class FormAddLecturer : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormAddLecturer()
        {
            InitializeComponent();
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync("api/STE/institutes").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    Institute[] institute = JsonConvert.DeserializeObject<Institute[]>(res.Result);
                    guna2ComboBox1.DataSource = institute;
                    guna2ComboBox1.DisplayMember = "Name";
                    guna2ComboBox1.ValueMember = "Id";
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch
            {
                MessageBox.Show("Lecturer: Не удалось загрузить список институтов", "Ошибка добавления группы студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            draggingAddModel = false;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draggingAddModel = true;
            dragCursorPointAddModel = Cursor.Position;
            dragFormPointAddModel = this.Location;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingAddModel)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPointAddModel));
                this.Location = Point.Add(dragFormPointAddModel, new Size(dif));
            }
        }

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text) 
                    || string.IsNullOrEmpty(guna2TextBox2.Text)
                    || string.IsNullOrEmpty(guna2TextBox3.Text)
                    || string.IsNullOrEmpty(guna2TextBox4.Text))
                {
                    MessageBox.Show("Поля должны быть заполнены", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    string shortName = guna2TextBox1.Text;
                    string surname = guna2TextBox2.Text;
                    string name = guna2TextBox3.Text;
                    string patronomyc = guna2TextBox4.Text;
                    Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
                    Lecturer lect = new Lecturer()
                    {
                        Shortname = shortName,
                        Surname = surname,
                        Name = name,
                        Patronomyc = patronomyc,
                        InstitutesId = selectedState.Id,
                        Institutes = selectedState,
                    };
                    FormListOfLecturers formListOfLecturers = this.Owner as FormListOfLecturers;
                    var data = JsonConvert.SerializeObject(lect);
                    //MessageBox.Show(data);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PostAsync("/api/STE/lecturer/save", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Преподаватель успешно добавлен!");
                        Lecturer lect_response = JsonConvert.DeserializeObject<Lecturer>(response.Content.ReadAsStringAsync().Result);
                        var rowIndex = formListOfLecturers.guna2DataGridView1.Rows.Add();
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = lect_response.Id;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnShortName"].Value = lect_response.Shortname;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnSurname"].Value = lect_response.Surname;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnName"].Value = lect_response.Name;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnPatronomyc"].Value = lect_response.Patronomyc;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = lect_response.Institutes.Name;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        //string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Ошибка: {response.StatusCode}\n{errorMessage}");
                        MessageBox.Show("Не удалось сохранить преподавателя", "Ошибка добавления преподавателя",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            this.Close();
        }
    }
}
