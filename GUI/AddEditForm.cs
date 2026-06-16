using System;
using System.Windows.Forms;
using TaskTracker.Core.Models;

namespace TaskTracker.Gui;
public partial class AddEditForm : Form
{
    public string Title { get; private set; }
    public string Description { get; private set; }

    private TextBox txtTitle;
    private TextBox txtDescription;
    private Button btnOk;
    private Button btnCancel;

    public AddEditForm(TaskItem? task = null)
    {
        InitializeComponent();
        if (task != null)
        {
            txtTitle.Text = task.Title;
            txtDescription.Text = task.Description;
            this.Text = "Редактировать задачу";
        }
        else
        {
            this.Text = "Добавить задачу";
        }
    }

    private void InitializeComponent()
    {
        this.Size = new System.Drawing.Size(400, 200);
        txtTitle = new TextBox { Location = new System.Drawing.Point(10, 30), Width = 350 };
        txtDescription = new TextBox { Location = new System.Drawing.Point(10, 70), Width = 350, Height = 60, Multiline = true };
        btnOk = new Button { Text = "OK", Location = new System.Drawing.Point(180, 130), DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Отмена", Location = new System.Drawing.Point(270, 130), DialogResult = DialogResult.Cancel };
        this.Controls.AddRange(new Control[] { txtTitle, txtDescription, btnOk, btnCancel });
        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;

        btnOk.Click += (s, e) => {
            Title = txtTitle.Text;
            Description = txtDescription.Text;
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Название не может быть пустым");
                this.DialogResult = DialogResult.None;
            }
        };
    }
}