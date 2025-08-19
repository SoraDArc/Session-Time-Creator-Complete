using Guna.UI2.WinForms;
using Newtonsoft.Json;
using STCUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace STCUI
{
    public partial class FormCreate : Form
    {
        User _user = new User();

        public string accessToken;
        public string login;

        public int auditoryReserveId;
        public string tempName;
        private int scheduleTemplateId;

        public bool IsCreated = false;
        public bool IsAccepted = false;
        public bool IsRejected = false;
        public FormCreate(string at, string l)
        {
            accessToken = at;
            login = l;
            InitializeComponent();
            guna2DataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            HttpClient client1 = new HttpClient();
            client1.BaseAddress = new Uri("http://localhost:5000/");
            client1.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response1 = client1.GetAsync($"api/STE/getUserData/{login}").Result;
            _user = JsonConvert.DeserializeObject<User>(response1.Content.ReadAsStringAsync().Result);
        }

        private void btnCreateST_Click(object sender, EventArgs e)
        {
            using (var model = new FormDataInput(_user.Id, _user.InstitutesId))
            {
                model.Owner = this;
                model.ShowDialog();
            }
            MessageBox.Show("Обязательно подтвердите или отмените создание, чтобы освободить/занять резервированные аудитории.", "Напоминание", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private async void btnAccept_Click(object sender, EventArgs e)
        {
            if (IsRejected)
            {
                return;
            }
            if (IsCreated == false || IsAccepted == true)
            {
                return;
            }
            IsAccepted = true;

            panel2.Visible = false;
            btnReject.Visible = false;
            panel4.Visible = true;
            btnToExcel.Visible = true;

            List<STElement> elementsList = new List<STElement>();
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
                elementsList.Add(temp);
            }
            ScheduleTemplate schedTemplate  = new ScheduleTemplate();
            schedTemplate.Name = tempName;
            var jsonTemp = JsonConvert.SerializeObject(elementsList);
            schedTemplate.JsonTemplate = jsonTemp;

            schedTemplate.OwnerUserId = _user.Id;
            schedTemplate.InstitutesId = _user.InstitutesId;
            
            ScheduleInformation scTemp = new ScheduleInformation();
            scTemp.AuditoryReserveId = auditoryReserveId;
            scTemp.ScTemplate = schedTemplate;
            var jsonData = JsonConvert.SerializeObject(scTemp);
            bool Confirment = true;
            var jsonConfirment = JsonConvert.SerializeObject(Confirment);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(jsonData, Encoding.UTF8), "scheduleInformation");
            formData.Add(new StringContent(jsonConfirment, Encoding.UTF8), "confirment");
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            HttpResponseMessage response = await client.PostAsync($"api/STE/confirmentST", formData);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Расписание подтверждено.");
                var res = response.Content.ReadAsStringAsync();
                int schTempId = JsonConvert.DeserializeObject<int>(res.Result);
                scheduleTemplateId = schTempId;
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString(), "Ошибка в подтверждении расписания (Accept).", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private async void btnReject_Click(object sender, EventArgs e)
        {
            if (IsAccepted)
            {
                return;
            }
            if (IsCreated == false || IsRejected == true)
            {
                return;
            }
            IsRejected = true;
            ScheduleTemplate schedTemplate = new ScheduleTemplate();
            schedTemplate.Name = tempName;

            HttpClient client1 = new HttpClient();
            client1.BaseAddress = new Uri("http://localhost:5000/");
            client1.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response1 = client1.GetAsync($"api/STE/getUserData/{login}").Result;
            User user = JsonConvert.DeserializeObject<User>(response1.Content.ReadAsStringAsync().Result);

            schedTemplate.OwnerUserId = user.Id;
            schedTemplate.InstitutesId = user.InstitutesId;

            ScheduleInformation scTemp = new ScheduleInformation();
            scTemp.AuditoryReserveId = auditoryReserveId;
            scTemp.ScTemplate = schedTemplate;

            var jsonData = JsonConvert.SerializeObject(scTemp);
            bool Confirment = false;
            var jsonConfirment = JsonConvert.SerializeObject(Confirment);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(jsonData, Encoding.UTF8), "scheduleInformation");
            formData.Add(new StringContent(jsonConfirment, Encoding.UTF8), "confirment");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            HttpResponseMessage response = await client.PostAsync($"api/STE/confirmentST", formData);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Расписание удалено.");
                guna2DataGridView1.Rows.Clear();
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString(), "Ошибка в подтверждении расписания (Reject).", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private async void btnToExcel_Click(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            HttpResponseMessage response = await client.GetAsync($"api/STE/exportToExcel/{scheduleTemplateId}");
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

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "ColumnDelete")
            {
                if (MessageBox.Show("Вы хотите удалить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    guna2DataGridView1.Rows.RemoveAt(e.RowIndex);
                }
            }
        }
    }
}
