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
    public partial class FormEditInstitute : Form
    {
        bool draggingEditModel = false;
        Point dragCursorPointEditModel;
        Point dragFormPointEditModel;
        public FormEditInstitute(string name, string shortName)
        {
            InitializeComponent();
            guna2TextBox1.Text = name;
            guna2TextBox2.Text = shortName;
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
            FormListOfInstitutes formListOfInstitutes = this.Owner as FormListOfInstitutes;
            var rowIndex = formListOfInstitutes.guna2DataGridView1.CurrentRow.Index;
            var id = (int)formListOfInstitutes.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value;
            var name = guna2TextBox1.Text;
            var shortName = guna2TextBox2.Text;
            var inst = new Institute() { Id = id, Name = name, ShortName = shortName };
            //MessageBox.Show($"{gos.Id}, {gos.Title} {gos.Institutes.Name} {gos.InstitutesId}");
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text) || string.IsNullOrEmpty(guna2TextBox2.Text))
                {
                    MessageBox.Show("Поля не могут быть пустыми", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    var data = JsonConvert.SerializeObject(inst);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PutAsync("/api/STE/institutes/edit", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Институт успешно изменен!");
                        Institute inst_response = JsonConvert.DeserializeObject<Institute>(response.Content.ReadAsStringAsync().Result);
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
                        MessageBox.Show("Не удалось изменить институт", "Ошибка изменения института",
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
