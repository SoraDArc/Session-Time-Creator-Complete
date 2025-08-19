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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Media;

namespace STCUI
{
    public partial class FormAddGroupsOfStudent : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormAddGroupsOfStudent()
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
                MessageBox.Show("GroupsOfStudent: Не удалось загрузить список институтов", "Ошибка добавления группы студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draggingAddModel = true;
            dragCursorPointAddModel = Cursor.Position;
            dragFormPointAddModel = this.Location;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            draggingAddModel = false;
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
                    MessageBox.Show("Поле с группой не может быть пустым", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    string title = guna2TextBox1.Text;
                    Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
                    GroupOfStudent g = new GroupOfStudent()
                    {
                        Title = title,
                        InstitutesId = selectedState.Id,
                        Institutes = selectedState,
                    };
                    FormListOfGroups formListOfGroups = this.Owner as FormListOfGroups;
                    var data = JsonConvert.SerializeObject(g);
                    //MessageBox.Show(data);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PostAsync("/api/STE/groupsOfStudent/save", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Группа успешно добавлена!");
                        GroupOfStudent gos_response = JsonConvert.DeserializeObject<GroupOfStudent>(response.Content.ReadAsStringAsync().Result);
                        var rowIndex = formListOfGroups.guna2DataGridView1.Rows.Add();
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            this.Close();
        }
    }
}
