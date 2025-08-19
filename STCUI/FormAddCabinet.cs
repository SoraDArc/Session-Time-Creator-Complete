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
    public partial class FormAddCabinet : Form
    {
        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        public FormAddCabinet()
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
                MessageBox.Show("Cabinet: Не удалось загрузить список институтов", "Ошибка добавления группы студентов", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
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
                    string type = guna2TextBox2.Text;
                    Institute selectedState = (Institute)guna2ComboBox1.SelectedItem;
                    Cabinet cab = new Cabinet()
                    {
                        Name = name,
                        Type = type,
                        InstitutesId = selectedState.Id,
                        Institutes = selectedState,
                    };
                    FormOfCabinets formListOfCabinets = this.Owner as FormOfCabinets;
                    var data = JsonConvert.SerializeObject(cab);
                    //MessageBox.Show(data);
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.PostAsync("/api/STE/cabinet/save", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Кабинет успешно добавлен!");
                        Cabinet cab_response = JsonConvert.DeserializeObject<Cabinet>(response.Content.ReadAsStringAsync().Result);
                        var rowIndex = formListOfCabinets.guna2DataGridView1.Rows.Add();
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
                        //string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Ошибка: {response.StatusCode}\n{errorMessage}");
                        MessageBox.Show("Не удалось сохранить новый кабинет", "Ошибка добавления кабинета",
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
