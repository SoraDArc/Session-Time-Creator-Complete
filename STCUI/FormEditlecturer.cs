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
    public partial class FormEditlecturer : Form
    {
        bool draggingEditModel = false;
        Point dragCursorPointEditModel;
        Point dragFormPointEditModel;
        public FormEditlecturer(string shortName, string surname, string name, string patronomyc, string inst)
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

                    var selectedInst = institute.FirstOrDefault(i => i.Name == inst);
                    if (selectedInst != null)
                    {
                        guna2ComboBox1.SelectedValue = selectedInst.Id;
                    }
                    guna2TextBox1.Text = shortName;
                    guna2TextBox2.Text = surname;
                    guna2TextBox3.Text = name;
                    guna2TextBox4.Text = patronomyc;
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information, 
                        MessageBoxDefaultButton.Button1, 
                        MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //MessageBox.Show("Не удалось загрузить список институтов", "Ошибка изменения группы студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ButtonAdd_Click(object sender, EventArgs e)
        {
            FormListOfLecturers formListOfLecturers = this.Owner as FormListOfLecturers;
            var rowIndex = formListOfLecturers.guna2DataGridView1.CurrentRow.Index;
            var id = (int)formListOfLecturers.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value;
            var shortName = guna2TextBox1.Text;
            var surname = guna2TextBox2.Text;
            var name = guna2TextBox3.Text;
            var patronomyc = guna2TextBox4.Text;
            Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
            var lect = new Lecturer() { Id = id, Shortname = shortName, Surname = surname, Name = name, Patronomyc = patronomyc, InstitutesId = selectedState.Id, Institutes = selectedState};
            //MessageBox.Show($"{gos.Id}, {gos.Title} {gos.Institutes.Name} {gos.InstitutesId}");
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text)
                    || string.IsNullOrEmpty(guna2TextBox2.Text)
                    || string.IsNullOrEmpty(guna2TextBox3.Text)
                    || string.IsNullOrEmpty(guna2TextBox4.Text))
                {
                    MessageBox.Show("Поля не могут быть пустыми", "Сообщение", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information, 
                        MessageBoxDefaultButton.Button1);
                }
                else
                {
                    var data = JsonConvert.SerializeObject(lect);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PutAsync("/api/STE/lecturer/edit", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Группа успешно изменена!");
                        Lecturer lect_response = JsonConvert.DeserializeObject<Lecturer>(response.Content.ReadAsStringAsync().Result);
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
                        MessageBox.Show("Не удалось изменить данные преподавателя", "Ошибка изменения данных преподавателя",
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

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            draggingEditModel = false;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draggingEditModel = true;
            dragCursorPointEditModel = Cursor.Position;
            dragFormPointEditModel = this.Location;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingEditModel)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPointEditModel));
                this.Location = Point.Add(dragFormPointEditModel, new Size(dif));
            }
        }
    }
}
