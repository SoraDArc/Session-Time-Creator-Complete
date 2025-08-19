using Guna.UI2.WinForms;
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
    public partial class FormAddInstitute : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormAddInstitute()
        {
            InitializeComponent();
        }

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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

        private void guna2ButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text) || string.IsNullOrEmpty(guna2TextBox2.Text))
                {
                    MessageBox.Show("Поля не могут быть пустыми", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    string name = guna2TextBox1.Text;
                    string shortName = guna2TextBox2.Text;
                    Institute inst = new Institute()
                    {
                        Name = name,
                        ShortName = shortName
                    };
                    FormListOfInstitutes formListOfInstitutes = this.Owner as FormListOfInstitutes;
                    var data = JsonConvert.SerializeObject(inst);
                    //MessageBox.Show(data);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PostAsync("/api/STE/institutes/save", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Институт успешно добавлен!");
                        Institute inst_response = JsonConvert.DeserializeObject<Institute>(response.Content.ReadAsStringAsync().Result);
                        var rowIndex = formListOfInstitutes.guna2DataGridView1.Rows.Add();
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = inst_response.Id;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = inst_response.Name;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnFullInstitute"].Value = inst_response.ShortName;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        //string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Ошибка: {response.StatusCode}\n{errorMessage}");
                        MessageBox.Show("Не удалось сохранить новую группу", "Ошибка добавления группы студентов",
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
