using System;
using System.Windows.Forms;
using VillainLairManager.Services;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using System.Linq;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Minion management form - STUB for students to implement
    /// Should contain CRUD operations with business logic in event handlers
    /// </summary>
    public partial class MinionManagementForm : Form
    {
        private readonly IRepository _repository;
        private readonly IMinionService _minionService;

        private Label lblTitle;
        private DataGridView dgvMinions;
        
        private Label lblName;
        private TextBox txtName;
        private Label lblSpecialty;
        private ComboBox cmbSpecialty;
        private Label lblSkillLevel;
        private TextBox txtSkillLevel;
        private Label lblSalary;
        private TextBox txtSalary;
        
        private Label lblBase;
        private ComboBox cmbBase;
        private Label lblScheme;
        private ComboBox cmbScheme;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;

        public MinionManagementForm(IRepository repository, IMinionService minionService)
        {
            _repository = repository;
            _minionService = minionService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Minion Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title
            this.lblTitle = new Label();
            this.lblTitle.Text = "Minions";
            this.lblTitle.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.AutoSize = true;

            // DataGridView
            this.dgvMinions = new DataGridView();
            this.dgvMinions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMinions.Location = new System.Drawing.Point(12, 40);
            this.dgvMinions.Name = "dgvMinions";
            this.dgvMinions.Size = new System.Drawing.Size(860, 300);
            this.dgvMinions.TabIndex = 0;
            this.dgvMinions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMinions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMinions.MultiSelect = false;
            this.dgvMinions.SelectionChanged += DgvMinions_SelectionChanged;

            // Input Controls
            int labelX = 12;
            int inputX = 160;
            int col2LabelX = 400;
            int col2InputX = 550;
            int startY = 360;
            int spacing = 30;

            // Name
            this.lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(labelX, startY), AutoSize = true };
            this.txtName = new TextBox { Location = new System.Drawing.Point(inputX, startY), Width = 200 };

            // Specialty
            this.lblSpecialty = new Label { Text = "Specialty:", Location = new System.Drawing.Point(col2LabelX, startY), AutoSize = true };
            this.cmbSpecialty = new ComboBox { Location = new System.Drawing.Point(col2InputX, startY), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Skill Level
            this.lblSkillLevel = new Label { Text = "Skill Level:", Location = new System.Drawing.Point(labelX, startY + spacing), AutoSize = true };
            this.txtSkillLevel = new TextBox { Location = new System.Drawing.Point(inputX, startY + spacing), Width = 200 };

            // Salary
            this.lblSalary = new Label { Text = "Salary:", Location = new System.Drawing.Point(col2LabelX, startY + spacing), AutoSize = true };
            this.txtSalary = new TextBox { Location = new System.Drawing.Point(col2InputX, startY + spacing), Width = 200 };

            // Base Assignment
            this.lblBase = new Label { Text = "Base Assignment:", Location = new System.Drawing.Point(labelX, startY + spacing * 2), AutoSize = true };
            this.cmbBase = new ComboBox { Location = new System.Drawing.Point(inputX, startY + spacing * 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Scheme Assignment
            this.lblScheme = new Label { Text = "Scheme Assignment:", Location = new System.Drawing.Point(col2LabelX, startY + spacing * 2), AutoSize = true };
            this.cmbScheme = new ComboBox { Location = new System.Drawing.Point(col2InputX, startY + spacing * 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Buttons
            int btnY = 480;
            int btnWidth = 100;
            int btnSpacing = 20;
            int startBtnX = (this.ClientSize.Width - (4 * btnWidth + 3 * btnSpacing)) / 2;

            this.btnAdd = new Button { Text = "Add", Location = new System.Drawing.Point(startBtnX, btnY), Size = new System.Drawing.Size(btnWidth, 30) };
            this.btnAdd.Click += BtnAdd_Click;
            this.btnUpdate = new Button { Text = "Update", Location = new System.Drawing.Point(startBtnX + btnWidth + btnSpacing, btnY), Size = new System.Drawing.Size(btnWidth, 30) };
            this.btnUpdate.Click += BtnUpdate_Click;
            this.btnDelete = new Button { Text = "Delete", Location = new System.Drawing.Point(startBtnX + 2 * (btnWidth + btnSpacing), btnY), Size = new System.Drawing.Size(btnWidth, 30) };
            this.btnDelete.Click += BtnDelete_Click;
            this.btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(startBtnX + 3 * (btnWidth + btnSpacing), btnY), Size = new System.Drawing.Size(btnWidth, 30) };
            this.btnRefresh.Click += BtnRefresh_Click;

            this.Controls.AddRange(new Control[] { 
                lblTitle, dgvMinions, 
                lblName, txtName, lblSpecialty, cmbSpecialty,
                lblSkillLevel, txtSkillLevel, lblSalary, txtSalary,
                lblBase, cmbBase, lblScheme, cmbScheme,
                btnAdd, btnUpdate, btnDelete, btnRefresh
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadReferenceData();
            LoadMinions();
        }

        private void LoadReferenceData()
        {
            try
            {
                cmbSpecialty.DataSource = ConfigManager.ValidSpecialties;
                cmbSpecialty.SelectedIndex = -1;

                var bases = _repository.GetAllBases();
                cmbBase.DataSource = bases;
                cmbBase.DisplayMember = "Name";
                cmbBase.ValueMember = "BaseId";
                cmbBase.SelectedIndex = -1;

                var schemes = _repository.GetAllSchemes();
                cmbScheme.DataSource = schemes;
                cmbScheme.DisplayMember = "Name";
                cmbScheme.ValueMember = "SchemeId";
                cmbScheme.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reference data: {ex.Message}");
            }
        }

        private void LoadMinions()
        {
            try
            {
                var minions = _minionService.GetAllMinions();
                dgvMinions.DataSource = minions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading minions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvMinions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMinions.SelectedRows.Count > 0)
            {
                var minion = dgvMinions.SelectedRows[0].DataBoundItem as Minion;
                if (minion != null)
                {
                    txtName.Text = minion.Name;
                    cmbSpecialty.SelectedItem = minion.Specialty;
                    txtSkillLevel.Text = minion.SkillLevel.ToString();
                    txtSalary.Text = minion.SalaryDemand.ToString();
                    
                    if (minion.CurrentBaseId.HasValue)
                        cmbBase.SelectedValue = minion.CurrentBaseId.Value;
                    else
                        cmbBase.SelectedIndex = -1;

                    if (minion.CurrentSchemeId.HasValue)
                        cmbScheme.SelectedValue = minion.CurrentSchemeId.Value;
                    else
                        cmbScheme.SelectedIndex = -1;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var minion = new Minion
                {
                    Name = txtName.Text,
                    Specialty = cmbSpecialty.Text,
                    SkillLevel = int.Parse(txtSkillLevel.Text),
                    SalaryDemand = decimal.Parse(txtSalary.Text),
                    CurrentBaseId = cmbBase.SelectedValue as int?,
                    CurrentSchemeId = cmbScheme.SelectedValue as int?,
                    MoodStatus = ConfigManager.MoodHappy,
                    LastMoodUpdate = DateTime.Now,
                    LoyaltyScore = 50 // Default starting loyalty
                };

                _minionService.CreateMinion(minion);
                LoadMinions();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMinions.SelectedRows.Count == 0) return;

            try
            {
                var selectedMinion = dgvMinions.SelectedRows[0].DataBoundItem as Minion;
                if (selectedMinion == null) return;

                selectedMinion.Name = txtName.Text;
                selectedMinion.Specialty = cmbSpecialty.Text;
                selectedMinion.SkillLevel = int.Parse(txtSkillLevel.Text);
                selectedMinion.SalaryDemand = decimal.Parse(txtSalary.Text);
                selectedMinion.CurrentBaseId = cmbBase.SelectedValue as int?;
                selectedMinion.CurrentSchemeId = cmbScheme.SelectedValue as int?;

                _minionService.UpdateMinion(selectedMinion);
                LoadMinions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMinions.SelectedRows.Count == 0) return;

            try
            {
                var selectedMinion = dgvMinions.SelectedRows[0].DataBoundItem as Minion;
                if (selectedMinion == null) return;

                if (MessageBox.Show($"Are you sure you want to delete {selectedMinion.Name}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _minionService.DeleteMinion(selectedMinion.MinionId);
                    LoadMinions();
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadMinions();
        }

        private void ClearInputs()
        {
            txtName.Clear();
            cmbSpecialty.SelectedIndex = -1;
            txtSkillLevel.Clear();
            txtSalary.Clear();
            cmbBase.SelectedIndex = -1;
            cmbScheme.SelectedIndex = -1;
        }
    }
}
