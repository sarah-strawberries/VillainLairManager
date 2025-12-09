using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public class SchemeEditDialog : Form
    {
        private readonly IRepository _repository;
        private readonly IEvilSchemeService _evilSchemeService;
        private EvilScheme _scheme;
        private bool _isNewScheme;

        // UI Controls
        private TextBox _txtName;
        private TextBox _txtDescription;
        private NumericUpDown _numDiabolicalRating;
        private NumericUpDown _numRequiredSkillLevel;
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpTargetDate;
        private TextBox _txtBudget;
        private ComboBox _cmbRequiredSpecialty;
        private ComboBox _cmbStatus;
        private Label _lblValidationMessage;
        private Button _btnSave;
        private Button _btnCancel;

        public SchemeEditDialog(IRepository repository, IEvilSchemeService evilSchemeService, EvilScheme scheme = null)
        {
            _repository = repository;
            _evilSchemeService = evilSchemeService;
            _scheme = scheme;
            _isNewScheme = (scheme == null);

            if (_isNewScheme)
            {
                _scheme = new EvilScheme
                {
                    SchemeId = 0,
                    Name = "",
                    Description = "",
                    DiabolicalRating = 5,
                    Status = "Planning",
                    Budget = 50000m,
                    CurrentSpending = 0m,
                    TargetCompletionDate = DateTime.Now.AddMonths(3),
                    RequiredSpecialty = "Hacking",
                    SuccessLikelihood = 50
                };
            }

            InitializeComponent();
            LoadFormData();
        }

        private void InitializeComponent()
        {
            this.Text = _isNewScheme ? "Create New Evil Scheme" : "Edit Evil Scheme";
            this.Size = new Size(600, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            this.Controls.Add(pnlMain);

            int yPos = 10;
            const int labelWidth = 120;
            const int controlWidth = 400;

            // SECTION 1: Basic Information
            var lblBasicInfo = new Label
            {
                Text = "Basic Information",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 550,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblBasicInfo);
            yPos += 35;

            // Scheme Name
            var lblName = new Label
            {
                Text = "Scheme Name *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblName);

            _txtName = new TextBox
            {
                Location = new Point(140, yPos),
                Size = new Size(controlWidth, 25),
                Font = new Font("Arial", 9),
                MaxLength = 100
            };
            pnlMain.Controls.Add(_txtName);
            yPos += 35;

            // Description
            var lblDescription = new Label
            {
                Text = "Description *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.TopLeft
            };
            pnlMain.Controls.Add(lblDescription);

            _txtDescription = new TextBox
            {
                Location = new Point(140, yPos),
                Size = new Size(controlWidth, 80),
                Font = new Font("Arial", 9),
                Multiline = true,
                MaxLength = 500,
                AcceptsReturn = true
            };
            pnlMain.Controls.Add(_txtDescription);
            yPos += 95;

            // Diabolical Rating
            var lblRating = new Label
            {
                Text = "Diabolical Rating *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblRating);

            _numDiabolicalRating = new NumericUpDown
            {
                Location = new Point(140, yPos),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Font = new Font("Arial", 9)
            };
            pnlMain.Controls.Add(_numDiabolicalRating);
            yPos += 35;

            // Required Skill Level
            var lblSkillLevel = new Label
            {
                Text = "Required Skill Level *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblSkillLevel);

            _numRequiredSkillLevel = new NumericUpDown
            {
                Location = new Point(140, yPos),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Font = new Font("Arial", 9)
            };
            pnlMain.Controls.Add(_numRequiredSkillLevel);
            yPos += 35;

            // SECTION 2: Timeline
            var lblTimeline = new Label
            {
                Text = "Timeline",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 550,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblTimeline);
            yPos += 35;

            // Start Date
            var lblStartDate = new Label
            {
                Text = "Start Date",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblStartDate);

            _dtpStartDate = new DateTimePicker
            {
                Location = new Point(140, yPos),
                Size = new Size(200, 25),
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short
            };
            pnlMain.Controls.Add(_dtpStartDate);
            yPos += 35;

            // Target Completion Date
            var lblTargetDate = new Label
            {
                Text = "Target Date *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblTargetDate);

            _dtpTargetDate = new DateTimePicker
            {
                Location = new Point(140, yPos),
                Size = new Size(200, 25),
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short
            };
            pnlMain.Controls.Add(_dtpTargetDate);
            yPos += 35;

            // SECTION 3: Budget & Resources
            var lblBudgetResources = new Label
            {
                Text = "Budget & Resources",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 550,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblBudgetResources);
            yPos += 35;

            // Total Budget
            var lblBudget = new Label
            {
                Text = "Total Budget *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblBudget);

            _txtBudget = new TextBox
            {
                Location = new Point(140, yPos),
                Size = new Size(100, 25),
                Font = new Font("Arial", 9),
                Text = "50000"
            };
            pnlMain.Controls.Add(_txtBudget);
            yPos += 35;

            // Required Specialty
            var lblSpecialty = new Label
            {
                Text = "Required Specialty *",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblSpecialty);

            _cmbRequiredSpecialty = new ComboBox
            {
                Location = new Point(140, yPos),
                Size = new Size(controlWidth, 25),
                Font = new Font("Arial", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbRequiredSpecialty.Items.AddRange(new[] { "Hacking", "Combat", "Explosives", "Disguise", "Infiltration" });
            pnlMain.Controls.Add(_cmbRequiredSpecialty);
            yPos += 35;

            // SECTION 4: Status (only show for existing schemes)
            if (!_isNewScheme)
            {
                var lblStatusSection = new Label
                {
                    Text = "Status",
                    Font = new Font("Arial", 11, FontStyle.Bold),
                    Location = new Point(10, yPos),
                    BackColor = Color.LightGray,
                    Width = 550,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                pnlMain.Controls.Add(lblStatusSection);
                yPos += 35;

                var lblStatus = new Label
                {
                    Text = "Status *",
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    Location = new Point(10, yPos),
                    Size = new Size(labelWidth, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                pnlMain.Controls.Add(lblStatus);

                _cmbStatus = new ComboBox
                {
                    Location = new Point(140, yPos),
                    Size = new Size(200, 25),
                    Font = new Font("Arial", 9),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                _cmbStatus.Items.AddRange(new[] { "Planning", "Active", "On Hold", "Completed", "Failed" });
                pnlMain.Controls.Add(_cmbStatus);
                yPos += 35;
            }

            // Validation Message Area
            _lblValidationMessage = new Label
            {
                Location = new Point(10, yPos),
                Size = new Size(540, 60),
                Font = new Font("Arial", 9),
                ForeColor = Color.Red,
                AutoSize = false,
                BackColor = Color.FromArgb(255, 240, 240),
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                TextAlign = ContentAlignment.TopLeft
            };
            pnlMain.Controls.Add(_lblValidationMessage);
            yPos += 70;

            // Buttons
            var pnlButtons = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(540, 50),
                BackColor = Color.White
            };
            pnlMain.Controls.Add(pnlButtons);

            _btnSave = new Button
            {
                Text = "Save",
                Location = new Point(380, 10),
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                Font = new Font("Arial", 9),
                DialogResult = DialogResult.None
            };
            _btnSave.Click += BtnSave_Click;
            pnlButtons.Controls.Add(_btnSave);

            _btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(465, 10),
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                Font = new Font("Arial", 9),
                DialogResult = DialogResult.Cancel
            };
            pnlButtons.Controls.Add(_btnCancel);
        }

        private void LoadFormData()
        {
            _txtName.Text = _scheme.Name;
            _txtDescription.Text = _scheme.Description;
            _numDiabolicalRating.Value = _scheme.DiabolicalRating;
            _numRequiredSkillLevel.Value = _scheme.RequiredSkillLevel > 0 ? _scheme.RequiredSkillLevel : 5;
            _dtpStartDate.Value = _scheme.CreatedDate != DateTime.MinValue ? _scheme.CreatedDate : DateTime.Now;
            _dtpTargetDate.Value = _scheme.TargetCompletionDate;
            _txtBudget.Text = _scheme.Budget.ToString("F2");
            _cmbRequiredSpecialty.SelectedItem = _scheme.RequiredSpecialty;

            if (_cmbStatus != null)
            {
                _cmbStatus.SelectedItem = _scheme.Status;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Clear previous validation message
            _lblValidationMessage.Visible = false;

            // Validate required fields
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_txtName.Text))
                errors.Add("• Scheme name is required");

            if (string.IsNullOrWhiteSpace(_txtDescription.Text))
                errors.Add("• Description is required");

            if (!decimal.TryParse(_txtBudget.Text, out decimal budget) || budget <= 0)
                errors.Add("• Budget must be a valid positive number");

            if (_cmbRequiredSpecialty.SelectedItem == null)
                errors.Add("• Required specialty must be selected");

            if (_dtpTargetDate.Value <= DateTime.Now && !_isNewScheme)
                errors.Add("⚠ Warning: Target date is in the past");

            if (_dtpStartDate.Value > _dtpTargetDate.Value)
                errors.Add("• Start date cannot be after target date");

            // Business rules validation
            if (!errors.Any(e => e.Contains("Budget must be")))
            {
                var (budgetValid, budgetWarnings) = _evilSchemeService.ValidateBudgetValues(budget, _scheme.CurrentSpending);
                if (!budgetValid)
                {
                    errors.AddRange(budgetWarnings);
                }
            }

            // Show errors if any
            if (errors.Count > 0)
            {
                _lblValidationMessage.Text = "Validation Errors:\n" + string.Join("\n", errors);
                _lblValidationMessage.Visible = true;
                return;
            }

            // Update scheme object
            _scheme.Name = _txtName.Text;
            _scheme.Description = _txtDescription.Text;
            _scheme.DiabolicalRating = (int)_numDiabolicalRating.Value;
            _scheme.RequiredSkillLevel = (int)_numRequiredSkillLevel.Value;
            _scheme.StartDate = _dtpStartDate.Value;
            _scheme.TargetCompletionDate = _dtpTargetDate.Value;
            _scheme.Budget = budget;
            _scheme.RequiredSpecialty = _cmbRequiredSpecialty.SelectedItem.ToString();

            if (_cmbStatus != null && _cmbStatus.SelectedItem != null)
            {
                string newStatus = _cmbStatus.SelectedItem.ToString();
                var (canTransition, transitionErrors) = _evilSchemeService.CanTransitionToStatus(_scheme.SchemeId, newStatus);

                if (!canTransition)
                {
                    _lblValidationMessage.Text = "Status Transition Error:\n" + string.Join("\n", transitionErrors);
                    _lblValidationMessage.Visible = true;
                    return;
                }

                _scheme.Status = newStatus;
            }

            // Save to database
            if (_isNewScheme)
            {
                _repository.InsertScheme(_scheme);
            }
            else
            {
                _repository.UpdateScheme(_scheme);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public EvilScheme GetScheme()
        {
            return _scheme;
        }
    }
}
