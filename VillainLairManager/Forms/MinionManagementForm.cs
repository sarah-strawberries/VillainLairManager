using System;
using System.Windows.Forms;
using VillainLairManager.Services;
using VillainLairManager.Models;
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
        private TextBox txtSpecialty;
        private Label lblSkillLevel;
        private TextBox txtSkillLevel;
        private Label lblSalary;
        private TextBox txtSalary;
        
        private Label lblBase;
        private ComboBox cmbBase;
        private Label lblScheme;
        private ComboBox cmbScheme;

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
            this.txtSpecialty = new TextBox { Location = new System.Drawing.Point(col2InputX, startY), Width = 200 };

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

            this.Controls.AddRange(new Control[] { 
                lblTitle, dgvMinions, 
                lblName, txtName, lblSpecialty, txtSpecialty,
                lblSkillLevel, txtSkillLevel, lblSalary, txtSalary,
                lblBase, cmbBase, lblScheme, cmbScheme
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
                    txtSpecialty.Text = minion.Specialty;
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
    }
}
