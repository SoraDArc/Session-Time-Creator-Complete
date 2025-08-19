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
    public partial class FormListOfSubjects : Form
    {
        public FormListOfSubjects()
        {
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.AllowUserToAddRows = false;
            Subject[] sub = new Subject[] { };
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync("api/STE/subject").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    sub = JsonConvert.DeserializeObject<Subject[]>(res.Result);
                    foreach (var s in sub)
                    {
                        int rowIndex = guna2DataGridView1.Rows.Add();
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = s.Id;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnSubject"].Value = s.Name;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = s.Institutes.Name;
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
                MessageBox.Show("ListOfSubjects: Сервер не отвечает", "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            using (var model = new FormAddSubject())
            {
                model.Owner = this;
                model.ShowDialog();
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
                        HttpResponseMessage response = client.DeleteAsync($"api/STE/subject/delete/{CellIndex}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Дисциплина успешно удалена!");
                            guna2DataGridView1.Rows.RemoveAt(e.RowIndex);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить дисциплину", "Ошибка удаления дисциплины",
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
                    var name = (string)cellsInfo[1].Value;
                    var institute = (string)cellsInfo[2].Value;

                    using (var model = new FormEditSubject(name, institute))
                    {
                        model.Owner = this;
                        model.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
