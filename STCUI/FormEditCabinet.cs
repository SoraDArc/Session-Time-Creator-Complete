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
    public partial class FormEditCabinet : Form
    {
        bool draggingEditModel = false;
        Point dragCursorPointEditModel;
        Point dragFormPointEditModel;

        public FormEditCabinet(string name, string type, string inst)
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
                    guna2TextBox2.Text = type;
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

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ButtonEdit_Click(object sender, EventArgs e)
        {
            FormOfCabinets formListOfCabinets = this.Owner as FormOfCabinets;
            var rowIndex = formListOfCabinets.guna2DataGridView1.CurrentRow.Index;
            var id = (int)formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value;
            var name = guna2TextBox1.Text;
            var type = guna2TextBox2.Text;
            Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
            var cab = new Cabinet() { Id = id, Name = name, Type = type, InstitutesId = selectedState.Id, Institutes = selectedState };
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text) || string.IsNullOrEmpty(guna2TextBox2.Text))
                {
                    MessageBox.Show("Поля не могут быть пустыми", "Сообщение", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information, 
                        MessageBoxDefaultButton.Button1);
                }
                else
                {
                    var data = JsonConvert.SerializeObject(cab);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PutAsync("/api/STE/cabinet/edit", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Аудитория успешно изменена!");
                        Cabinet cab_response = JsonConvert.DeserializeObject<Cabinet>(response.Content.ReadAsStringAsync().Result);
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = cab_response.Id;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnCabinet"].Value = cab_response.Name;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnType"].Value = cab_response.Type;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = cab_response.Institutes.Name;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formListOfCabinets.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить аудиторию", "Ошибка изменения аудитории",
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
