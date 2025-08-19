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
using System.Xml.Linq;

namespace STCUI
{
    public partial class FormEditSubject : Form
    {
        bool draggingEditModel = false;
        Point dragCursorPointEditModel;
        Point dragFormPointEditModel;
        public FormEditSubject(string name, string inst)
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
                    guna2TextBox1.Text = name;
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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

        private void guna2ButtonEdit_Click(object sender, EventArgs e)
        {
            FormListOfSubjects formListOfSubjects = this.Owner as FormListOfSubjects;
            var rowIndex = formListOfSubjects.guna2DataGridView1.CurrentRow.Index;
            var id = (int)formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value;
            var name = guna2TextBox1.Text;
            Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
            var gos = new Subject() { Id = id, Name = name, InstitutesId = selectedState.Id, Institutes = selectedState };
            //MessageBox.Show($"{gos.Id}, {gos.Title} {gos.Institutes.Name} {gos.InstitutesId}");
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text))
                {
                    MessageBox.Show("Поле с дисциплинной не может быть пустым", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    var data = JsonConvert.SerializeObject(gos);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PutAsync("/api/STE/subject/edit", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Дисциплинна успешно изменена!");
                        Subject sub_response = JsonConvert.DeserializeObject<Subject>(response.Content.ReadAsStringAsync().Result);
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = sub_response.Id;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnSubject"].Value = sub_response.Name;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = sub_response.Institutes.Name;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formListOfSubjects.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить дисциплину", "Ошибка изменения дисциплины",
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
