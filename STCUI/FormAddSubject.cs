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
    public partial class FormAddSubject : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormAddSubject()
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
                MessageBox.Show("Subject: Не удалось загрузить список институтов", "Ошибка добавления студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
                if (string.IsNullOrEmpty(guna2TextBox1.Text))
                {
                    MessageBox.Show("Поле с дисциплинной не может быть пустым", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    string name = guna2TextBox1.Text;
                    Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
                    Subject g = new Subject()
                    {
                        Name = name,
                        InstitutesId = selectedState.Id,
                        Institutes = selectedState,
                    };
                    FormListOfSubjects formListOfSubjects = this.Owner as FormListOfSubjects;
                    var data = JsonConvert.SerializeObject(g);
                    //MessageBox.Show(data);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PostAsync("/api/STE/subject/save", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Дисциплина успешно добавлена!");
                        Subject sub_response = JsonConvert.DeserializeObject<Subject>(response.Content.ReadAsStringAsync().Result);
                        var rowIndex = formListOfSubjects.guna2DataGridView1.Rows.Add();
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
                        //string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Ошибка: {response.StatusCode}\n{errorMessage}");
                        MessageBox.Show("Не удалось сохранить новую дисциплину", "Ошибка добавления дисциплины",
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
