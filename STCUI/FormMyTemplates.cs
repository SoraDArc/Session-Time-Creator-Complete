using Newtonsoft.Json;
using STCUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace STCUI
{
    public partial class FormMyTemplates : Form
    {
        private List<ScheduleInformation> _scheduleInformation;
        public string accessToken;
        public string login;
        public FormMyTemplates(string at, string l)
        {
            accessToken = at;
            login = l;
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            try
            {
                HttpClient client1 = new HttpClient();
                client1.BaseAddress = new Uri("http://localhost:5000/");
                client1.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response1 = client1.GetAsync($"api/STE/getUserData/{login}").Result;
                User user = JsonConvert.DeserializeObject<User>(response1.Content.ReadAsStringAsync().Result);
                int id = user.Id;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.GetAsync($"api/STE/myTemplates/{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    ScheduleInformation[] scheduleInformation = JsonConvert.DeserializeObject<ScheduleInformation[]>(res.Result);
                    this._scheduleInformation = scheduleInformation.ToList();
                   
                    guna2ComboBox1.DisplayMember = "ScTemplate.Name";
                    guna2ComboBox1.ValueMember = "AuditoryReserveId";
                    guna2ComboBox1.DataSource = this._scheduleInformation;
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString(), "Ошибка в загрузке шаблонов пользователя.", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                //"ScheduleTemplate: Не удалось загрузить список шаблонов."
                MessageBox.Show(ex.ToString(), "Ошибка шаблонов пользователя.", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            guna2DataGridView1.Rows.Clear();
            ScheduleInformation selectedState = (ScheduleInformation)guna2ComboBox1.SelectedItem;
            List<STElement> ExamList = JsonConvert.DeserializeObject<List<STElement>>(selectedState.ScTemplate.JsonTemplate);
            for (int i = 0; i < ExamList.Count(); i++)
            {
                var rowIndex = guna2DataGridView1.Rows.Add();
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = i + 1;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnSubject"].Value = ExamList[i].Name;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnCourse"].Value = ExamList[i].Course;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnGroup"].Value = ExamList[i].Group;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnLecturer"].Value = ExamList[i].Lector;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnConsultationDate"].Value = ExamList[i].Consultation.ToString("dd.MM.yyyy");
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnExamDate"].Value = ExamList[i].Exam.ToString("dd.MM.yyyy");
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnAuditory"].Value = ExamList[i].Auditoria;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnWishDate"].Value = ExamList[i].WishDate;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnWishAuditoria"].Value = ExamList[i].WishAuditoria;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ScheduleInformation selectedState = (ScheduleInformation)guna2ComboBox1.SelectedItem;
            if (selectedState == null)
            {
                MessageBox.Show($"Ошибка: нет объекта для удаления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                if (MessageBox.Show($"Вы хотите удалить шаблон - {selectedState.ScTemplate.Name}", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    HttpResponseMessage response = client.DeleteAsync($"/api/STE/deleteMyTemplate/{selectedState.AuditoryReserveId}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Шаблон успешно удалена!");
                        this._scheduleInformation.Remove(selectedState);
                        guna2ComboBox1.DataSource = null;
                        guna2ComboBox1.DataSource = this._scheduleInformation;

                        guna2DataGridView1.Rows.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить шаблон", "Ошибка удаления шаблона",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ScheduleInformation selectedState = (ScheduleInformation)guna2ComboBox1.SelectedItem;
            List<STElement> ExamList = new List<STElement>();
            for (int i = 0; i < guna2DataGridView1.Rows.Count; i++)
            {
                if (guna2DataGridView1.Rows[i].IsNewRow) continue;

                var temp = new STElement();
                temp.Name = guna2DataGridView1.Rows[i].Cells["ColumnSubject"].Value.ToString();
                temp.Course = guna2DataGridView1.Rows[i].Cells["ColumnCourse"].Value.ToString();
                temp.Group = guna2DataGridView1.Rows[i].Cells["ColumnGroup"].Value.ToString();
                temp.Lector = guna2DataGridView1.Rows[i].Cells["ColumnLecturer"].Value.ToString();
                temp.Consultation = DateTime.Parse(guna2DataGridView1.Rows[i].Cells["ColumnConsultationDate"].Value.ToString());
                temp.Exam = DateTime.Parse(guna2DataGridView1.Rows[i].Cells["ColumnExamDate"].Value.ToString());
                temp.Auditoria = guna2DataGridView1.Rows[i].Cells["ColumnAuditory"].Value.ToString();
                temp.WishDate = guna2DataGridView1.Rows[i].Cells["ColumnWishDate"].Value.ToString();
                temp.WishAuditoria = guna2DataGridView1.Rows[i].Cells["ColumnWishAuditoria"].Value.ToString();
                ExamList.Add(temp);
            }

            selectedState.ScTemplate.JsonTemplate = JsonConvert.SerializeObject(ExamList);
            //MessageBox.Show($"selectedState:\nar_ID: {selectedState.AuditoryReserveId}\nname: {selectedState.ScTemplate.Name}");
            try
            {
                var data = JsonConvert.SerializeObject(selectedState);

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(data, Encoding.UTF8), "scheduleInformation");

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = client.PutAsync("/api/STE/updateMyTemplate", formData).Result;
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Шаблон успешно обновлён.");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {response.StatusCode}", "Ошибка в обновлении шаблонов пользователя", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnToExcel_Click(object sender, EventArgs e)
        {
            ScheduleInformation selectedState = (ScheduleInformation)guna2ComboBox1.SelectedItem;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            HttpResponseMessage response = await client.GetAsync($"api/STE/exportToExcel/{selectedState.ScTemplate.Id}");
            if (response.IsSuccessStatusCode)
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveDialog.FileName = "Template.xlsx";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    MessageBox.Show("Файл успешно сохранён!");
                }
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString(), "Ошибка в переносе данных в excel.", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }


    }
}
