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
    public partial class FormListOfInstitutes : Form
    {
        public FormListOfInstitutes()
        {
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.AllowUserToAddRows = false;
            Institute[] inst = new Institute[] { };
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync("api/STE/institutes").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    inst = JsonConvert.DeserializeObject<Institute[]>(res.Result);
                    foreach (var i in inst)
                    {
                        int rowIndex = guna2DataGridView1.Rows.Add();
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = i.Id;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = i.Name;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnFullInstitute"].Value = i.ShortName;
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
                MessageBox.Show("ListOfGroups: Сервер не отвечает", "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            using (var model = new FormAddInstitute())
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
                        HttpResponseMessage response = client.DeleteAsync($"api/STE/institutes/delete/{CellIndex}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Институт успешно удален!");
                            guna2DataGridView1.Rows.RemoveAt(e.RowIndex);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить институт", "Ошибка удаления института",
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
                    var FullName= (string)cellsInfo[2].Value;

                    using (var model = new FormEditInstitute(name, FullName))
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
