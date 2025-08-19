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
using System.Xml.Linq;

namespace STCUI
{
    public partial class FormOfCabinets : Form
    {
        public FormOfCabinets()
        {
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.AllowUserToAddRows = false;
            Cabinet[] cabs = new Cabinet[] { };
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync("api/STE/cabinet").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    cabs = JsonConvert.DeserializeObject<Cabinet[]>(res.Result);
                    foreach (var c in cabs)
                    {
                        int rowIndex = guna2DataGridView1.Rows.Add();
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = c.Id;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnCabinet"].Value = c.Name;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnType"].Value = c.Type;
                        guna2DataGridView1.Rows[rowIndex].Cells["ColumnInstitute"].Value = c.Institutes.Name;
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
                MessageBox.Show("ListOfCabinets: Сервер не отвечает", "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            using (var model = new FormAddCabinet())
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
                    if (MessageBox.Show("Вы хотите удалить?", "Подтверждение", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question, 
                        MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        var CellIndex = (int)guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value;
                        //MessageBox.Show(guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("http://localhost:5000/");
                        HttpResponseMessage response = client.DeleteAsync($"api/STE/cabinet/delete/{CellIndex}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Кабинет успешно удален!");
                            guna2DataGridView1.Rows.RemoveAt(e.RowIndex);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить кабинет", "Ошибка удаления кабинета",
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
                    var type = (string)cellsInfo[2].Value;
                    var institute = (string)cellsInfo[3].Value;

                    using (var model = new FormEditCabinet(name, type, institute))
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
