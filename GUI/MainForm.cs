using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TaskTracker.Core.Models;
using TaskTracker.Core.Services;
using TaskTracker.Core.Support;
using TaskTracker.Storage.Services;
using TaskTracker.Storage;

namespace TaskTracker.Gui;
public partial class MainForm : Form
{
    private TaskService _service;
    private ITaskStorage _storage;
    private AppLogger _logger;
    private DiagnosticsService _diagnostics;
    private string _dataFilePath;
    private string _reportsFolder;

    private DataGridView dgvTasks;
    private Button btnAdd, btnEdit, btnDelete, btnRestore, btnArchive, btnUnarchive, btnDiagnostics, btnReport, btnClearTrash;
    private ComboBox cbFilterStatus;
    private TextBox txtSearch;
    private Button btnSearch;
    private TabControl tabControl;
    private BindingSource bindingSource; // Добавляем BindingSource для надёжности

    public MainForm()
    {
        InitializeComponent();
        InitializeApp();
        LoadTasks();
        dgvTasks.DataBindingComplete += DgvTasks_DataBindingComplete;
    }

    private void InitializeComponent()
    {
        this.Text = "TaskTracker - Управление задачами";
        this.Size = new System.Drawing.Size(900, 600);

        tabControl = new TabControl { Dock = DockStyle.Fill };
        var tabActive = new TabPage("Активные задачи");
        var tabTrash = new TabPage("Корзина");
        var tabArchive = new TabPage("Архив");
        tabControl.TabPages.Add(tabActive);
        tabControl.TabPages.Add(tabTrash);
        tabControl.TabPages.Add(tabArchive);

        dgvTasks = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
        dgvTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTasks.MultiSelect = false;
        tabActive.Controls.Add(dgvTasks);

        bindingSource = new BindingSource(); // Создаём BindingSource
        dgvTasks.DataSource = bindingSource;

        var panel = new Panel { Dock = DockStyle.Bottom, Height = 110 };
        btnAdd = new Button { Text = "Добавить", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(100, 30) };
        btnEdit = new Button { Text = "Редактировать", Location = new System.Drawing.Point(120, 10), Size = new System.Drawing.Size(100, 30) };
        btnDelete = new Button { Text = "Удалить (в корзину)", Location = new System.Drawing.Point(230, 10), Size = new System.Drawing.Size(130, 30) };
        btnRestore = new Button { Text = "Восстановить", Location = new System.Drawing.Point(370, 10), Size = new System.Drawing.Size(100, 30) };
        btnArchive = new Button { Text = "Архивировать", Location = new System.Drawing.Point(480, 10), Size = new System.Drawing.Size(100, 30) };
        btnUnarchive = new Button { Text = "Вернуть из архива", Location = new System.Drawing.Point(590, 10), Size = new System.Drawing.Size(120, 30) };
        btnClearTrash = new Button { Text = "Очистить корзину", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(120, 30), BackColor = System.Drawing.Color.IndianRed };
        btnDiagnostics = new Button { Text = "Диагностика", Location = new System.Drawing.Point(140, 50), Size = new System.Drawing.Size(100, 30) };
        btnReport = new Button { Text = "Отчёт поддержки", Location = new System.Drawing.Point(250, 50), Size = new System.Drawing.Size(120, 30) };
        panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRestore, btnArchive, btnUnarchive, btnClearTrash, btnDiagnostics, btnReport });

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        txtSearch = new TextBox { Location = new System.Drawing.Point(10, 8), Width = 200 };
        btnSearch = new Button { Text = "Поиск", Location = new System.Drawing.Point(220, 7), Size = new System.Drawing.Size(75, 23) };
        cbFilterStatus = new ComboBox { Location = new System.Drawing.Point(310, 8), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        cbFilterStatus.Items.AddRange(new object[] { "Все", "New", "InProgress", "Done" });
        cbFilterStatus.SelectedIndex = 0;
        searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch, cbFilterStatus });

        this.Controls.Add(tabControl);
        this.Controls.Add(panel);
        this.Controls.Add(searchPanel);

        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnRestore.Click += BtnRestore_Click;
        btnArchive.Click += BtnArchive_Click;
        btnUnarchive.Click += BtnUnarchive_Click;
        btnClearTrash.Click += BtnClearTrash_Click;
        btnDiagnostics.Click += BtnDiagnostics_Click;
        btnReport.Click += BtnReport_Click;
        btnSearch.Click += BtnSearch_Click;
        cbFilterStatus.SelectedIndexChanged += (s, e) => LoadTasks();
        tabControl.SelectedIndexChanged += (s, e) => LoadTasks();
    }

    private void InitializeApp()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataFolder = Path.Combine(baseDir, "Data");
        var logsFolder = Path.Combine(baseDir, "Logs");
        _reportsFolder = Path.Combine(baseDir, "Reports");
        var backupsFolder = Path.Combine(baseDir, "Backups");
        var exportsFolder = Path.Combine(baseDir, "Exports");
        var configPath = Path.Combine(baseDir, "Config", "config.json");
        _dataFilePath = Path.Combine(dataFolder, "tasks.json");

        _storage = new JsonTaskStorage(_dataFilePath);
        var loadedTasks = _storage.Load();
        _service = new TaskService(loadedTasks);
        _logger = new AppLogger(logsFolder);
        _logger.Info("GUI приложение запущено");

        _diagnostics = new DiagnosticsService(baseDir, configPath, "Admin", "JSON", dataFolder, logsFolder, backupsFolder, exportsFolder, _reportsFolder, _dataFilePath);
    }

    private void LoadTasks()
    {
        var list = tabControl.SelectedIndex == 0 ? _service.GetAllActive()
                  : tabControl.SelectedIndex == 1 ? _service.GetTrash()
                  : _service.GetArchive();

        var filtered = list.AsEnumerable();
        if (tabControl.SelectedIndex == 0)
        {
            var searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
                filtered = filtered.Where(t => (t.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                                               (t.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
            var selectedStatus = cbFilterStatus.SelectedItem?.ToString();
            if (selectedStatus != null && selectedStatus != "Все")
                filtered = filtered.Where(t => t.Status.ToString() == selectedStatus);
        }

        bindingSource.DataSource = null;
        bindingSource.DataSource = filtered.ToList();
        dgvTasks.Refresh();
    }

    private void DgvTasks_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (dgvTasks.Columns.Contains("Id")) dgvTasks.Columns["Id"].Width = 50;
        if (dgvTasks.Columns.Contains("Title")) dgvTasks.Columns["Title"].Width = 200;
        if (dgvTasks.Columns.Contains("Description")) dgvTasks.Columns["Description"].Width = 250;
        if (dgvTasks.Columns.Contains("Status")) dgvTasks.Columns["Status"].Width = 80;
        if (dgvTasks.Columns.Contains("IsDeleted")) dgvTasks.Columns["IsDeleted"].Visible = false;
        if (dgvTasks.Columns.Contains("IsArchived")) dgvTasks.Columns["IsArchived"].Visible = false;
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        var form = new AddEditForm(null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _service.Add(form.Title, form.Description);
                _storage.Save(_service.GetAll());
                _logger.Info($"ADD title=\"{form.Title}\"");
                LoadTasks();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) { MessageBox.Show("Выберите задачу"); return; }
        var task = (TaskItem)dgvTasks.SelectedRows[0].DataBoundItem;
        var form = new AddEditForm(task);
        if (form.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _service.Update(task.Id, form.Title, form.Description);
                _storage.Save(_service.GetAll());
                _logger.Info($"UPDATE id={task.Id}");
                LoadTasks();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        var task = (TaskItem)dgvTasks.SelectedRows[0].DataBoundItem;

        if (tabControl.SelectedIndex == 0) // Активные -> в корзину
        {
            _service.Delete(task.Id);
            _logger.Info($"DELETE id={task.Id}");
        }
        else if (tabControl.SelectedIndex == 1) // Корзина -> удалить навсегда
        {
            if (MessageBox.Show($"Удалить задачу \"{task.Title}\" навсегда?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _service.PermanentDelete(task.Id);
                _logger.Info($"PERMANENT_DELETE id={task.Id}");
            }
        }
        _storage.Save(_service.GetAll());
        LoadTasks();
    }

    private void BtnRestore_Click(object sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        var task = (TaskItem)dgvTasks.SelectedRows[0].DataBoundItem;

        if (tabControl.SelectedIndex == 1) // Корзина -> восстановить
        {
            _service.Restore(task.Id);
            _logger.Info($"RESTORE id={task.Id}");
        }
        else if (tabControl.SelectedIndex == 2) // Архив -> вернуть из архива
        {
            _service.Unarchive(task.Id);
            _logger.Info($"UNARCHIVE id={task.Id}");
        }
        _storage.Save(_service.GetAll());
        LoadTasks();
    }

    private void BtnArchive_Click(object sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0 || tabControl.SelectedIndex != 0) return;
        var task = (TaskItem)dgvTasks.SelectedRows[0].DataBoundItem;
        try
        {
            _service.Archive(task.Id);
            _storage.Save(_service.GetAll());
            _logger.Info($"ARCHIVE id={task.Id}");
            LoadTasks();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnUnarchive_Click(object sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0 || tabControl.SelectedIndex != 2) return;
        var task = (TaskItem)dgvTasks.SelectedRows[0].DataBoundItem;
        try
        {
            _service.Unarchive(task.Id);
            _storage.Save(_service.GetAll());
            _logger.Info($"UNARCHIVE id={task.Id}");
            LoadTasks();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnClearTrash_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Очистить корзину (удалить все задачи навсегда)?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _service.ClearTrash();
            _storage.Save(_service.GetAll());
            _logger.Info("CLEAR_TRASH performed");
            LoadTasks();
            MessageBox.Show("Корзина очищена.");
        }
    }

    private void BtnDiagnostics_Click(object sender, EventArgs e)
    {
        var lines = _diagnostics.Run(_service);
        MessageBox.Show(string.Join(Environment.NewLine, lines), "Диагностика", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnReport_Click(object sender, EventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_reportsFolder);
            var lines = _diagnostics.Run(_service);
            var fileName = $"SupportReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var filePath = Path.Combine(_reportsFolder, fileName);
            File.WriteAllLines(filePath, lines);
            MessageBox.Show($"Отчёт создан: {filePath}", "Успех");
            _logger.Info($"SUPPORT_REPORT created {filePath}");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }

    private void BtnSearch_Click(object sender, EventArgs e) => LoadTasks();
}