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
    public partial class FormListOfLecturers : Form
    {
        public FormListOfLecturers()
        {
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.AllowUserToAddRows = false;
            Lecturer[] lecturers = new Lecturer[]{};
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync("api/STE/lecturer").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    lecturers = JsonConvert.DeserializeObject<Lecturer[]>(res.Result);
                    foreach (var l in lecturers)
                    {
                        int rowIndex = guna2DataGridView1.Rows.Add();
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = l.Id;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnShortName"].Value = l.Shortname;
                        if (string.IsNullOrEmpty(l.Surname))
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnSurname"].Value = "неизвестно";
                        }
                        else
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnSurname"].Value = l.Surname;
                        }

                        if (string.IsNullOrEmpty(l.Name))
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnName"].Value = "неизвестно";
                        }
                        else
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnName"].Value = l.Name;
                        }

                        if (string.IsNullOrEmpty(l.Patronomyc))
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnPatronomyc"].Value = "неизвестно";
                        }
                        else
                        {
                            guna2DataGridView1.Rows[rowIndex].Cells["ColumnPatronomyc"].Value = l.Patronomyc;
                        }

                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = l.Institutes.Name;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.BackColor = System.Drawing.Color.DarkCyan;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnEdit"].Style.ForeColor = System.Drawing.Color.White;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch
            {
                MessageBox.Show("ListOfLecturers: Сервер не отвечает", "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "ColumnDelete")
                {
                    if (MessageBox.Show("Вы хотите удалить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        var CellIndex = (int)guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value;
                        //MessageBox.Show(guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("http://localhost:5000/");
                        HttpResponseMessage response = client.DeleteAsync($"api/STE/lecturer/delete/{CellIndex}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Преподаватель успешно удален!");
                            guna2DataGridView1.Rows.RemoveAt(e.RowIndex);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить преподавателя", "Ошибка удаления преподавателя",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                }
                else if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "ColumnEdit")
                {
                    var cellsInfo = guna2DataGridView1.Rows[e.RowIndex].Cells;
                    var shortName = (string)cellsInfo[1].Value;
                    var surname = (string)cellsInfo[2].Value;
                    var name = (string)cellsInfo[3].Value;
                    var patronomyc = (string)cellsInfo[4].Value;
                    var institute = (string)cellsInfo[5].Value;

                    using (var model = new FormEditlecturer(shortName, surname, name, patronomyc, institute))
                    {
                        model.Owner = this;
                        model.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            using (var model = new FormAddLecturer())
            {
                model.Owner = this;
                model.ShowDialog();
            }
        }
    }
}
