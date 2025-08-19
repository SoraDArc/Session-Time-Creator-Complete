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
    public partial class FormEditGroupsOfStudent : Form
    {
        bool draggingEditModel = false;
        Point dragCursorPointEditModel;
        Point dragFormPointEditModel;
        public FormEditGroupsOfStudent(string title, string inst)
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
                    guna2TextBox1.Text = title;
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //MessageBox.Show("Не удалось загрузить список институтов", "Ошибка изменения группы студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

        }

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ButtonEdit_Click(object sender, EventArgs e)
        {
            FormListOfGroups formListOfGroups = this.Owner as FormListOfGroups;
            var rowIndex = formListOfGroups.guna2DataGridView1.CurrentRow.Index;
            var id = (int)formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value;
            var title = guna2TextBox1.Text;
            Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
            var gos = new GroupOfStudent() { Id = id, Title = title, InstitutesId = selectedState.Id, Institutes = selectedState };
            //MessageBox.Show($"{gos.Id}, {gos.Title} {gos.Institutes.Name} {gos.InstitutesId}");
            try
            {
                if (string.IsNullOrEmpty(guna2TextBox1.Text))
                {
                    MessageBox.Show("Поле с группой не может быть пустым", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    var data = JsonConvert.SerializeObject(gos);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PutAsync("/api/STE/groupsOfStudent/edit", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Группа успешно изменена!");
                        GroupOfStudent gos_response = JsonConvert.DeserializeObject<GroupOfStudent>(response.Content.ReadAsStringAsync().Result);
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = gos_response.Id;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnTitle"].Value = gos_response.Title;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = gos_response.Institutes.Name;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formListOfGroups.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить группу", "Ошибка изменения группы студентов",
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

        private void panel1_MouseUp_1(object sender, MouseEventArgs e)
        {
            draggingEditModel = false;
        }

        private void panel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            draggingEditModel = true;
            dragCursorPointEditModel = Cursor.Position;
            dragFormPointEditModel = this.Location;
        }

        private void panel1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (draggingEditModel)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPointEditModel));
                this.Location = Point.Add(dragFormPointEditModel, new Size(dif));
            }
        }
    }
}
