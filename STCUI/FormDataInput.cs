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

namespace STCUI
{
    public partial class FormDataInput : Form
    {
        public int UserId;
        public int InstituteId;

        bool draggingAddModel = false;
        Point dragCursorPointAddModel;
        Point dragFormPointAddModel;
        private string selectedFilePath;
        public FormDataInput(int userID, int instituteID)
        {
            UserId = userID;    
            InstituteId = instituteID;
            InitializeComponent();
        }

        private void chooseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                label1.Text = $"{Path.GetFileName(selectedFilePath)}";
            }
        }

        private async void guna2ButtonAdd_Click(object sender, EventArgs e)
        {
            string cabinets = richTextBox1.Text.Trim();
            string dateStart = dateTimePickerStart.Value.ToString("dd.MM.yyyy");
            string dateEnd = dateTimePickerEnd.Value.ToString("dd.MM.yyyy");
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Экскель файл не выбран.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                byte[] excelFileBytes = System.IO.File.ReadAllBytes(selectedFilePath);
                var fileContent = new ByteArrayContent(excelFileBytes);
                fileContent.Headers.ContentType =
                 new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                var formData = new MultipartFormDataContent();
                formData.Add(fileContent, "file", Path.GetFileName(selectedFilePath));
                formData.Add(new StringContent(cabinets, Encoding.UTF8), "cabinets");
                formData.Add(new StringContent(dateStart, Encoding.UTF8), "startDate");
                formData.Add(new StringContent(dateEnd, Encoding.UTF8), "endDate");
                formData.Add(new StringContent(UserId.ToString(), Encoding.UTF8), "userID");
                formData.Add(new StringContent(InstituteId.ToString(), Encoding.UTF8), "instituteID");

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5000/");
                HttpResponseMessage response = await client.PostAsync($"api/STE/createST", formData);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Расписание успешно сгенерировано.");
                    FormCreate formCreate = this.Owner as FormCreate;
                    formCreate.IsCreated = true;
                    ScheduleInformation schedInfo = JsonConvert.DeserializeObject<ScheduleInformation>(response.Content.ReadAsStringAsync().Result);
                    formCreate.auditoryReserveId = schedInfo.AuditoryReserveId;
                    formCreate.tempName = schedInfo.ScTemplate.Name;
                    var temp = schedInfo.ScTemplate.JsonTemplate;
                    List <STElement> ExamList = JsonConvert.DeserializeObject<List<STElement>>(temp);
                    for (int i = 0; i < ExamList.Count(); i++)
                    {
                        var rowIndex = formCreate.guna2DataGridView1.Rows.Add();
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnId"].Value = i + 1;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnSubject"].Value = ExamList[i].Name;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnCourse"].Value = ExamList[i].Course;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnGroup"].Value = ExamList[i].Group;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnLecturer"].Value = ExamList[i].Lector;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnConsultationDate"].Value = ExamList[i].Consultation.ToString("dd.MM.yyyy");
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnExamDate"].Value = ExamList[i].Exam.ToString("dd.MM.yyyy");
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnAuditory"].Value = ExamList[i].Auditoria;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnWishDate"].Value = ExamList[i].WishDate;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnWishAuditoria"].Value = ExamList[i].WishAuditoria;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.BackColor = System.Drawing.Color.Maroon;
                        formCreate.guna2DataGridView1.Rows[rowIndex].Cells["ColumnDelete"].Style.ForeColor = System.Drawing.Color.White;
                    }
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при генерации расписания: {error}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка в кнопке создания расписания", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            this.Close();
        }

        private void guna2ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draggingAddModel = true;
            dragCursorPointAddModel = Cursor.Position;
            dragFormPointAddModel = this.Location;
        }
    }
}
