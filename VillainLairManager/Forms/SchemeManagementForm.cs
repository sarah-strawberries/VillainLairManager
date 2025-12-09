using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class SchemeManagementForm : Form
    {
        private readonly IRepository _repository;
        private readonly IEvilSchemeService _evilSchemeService;

        // UI Controls
        private DataGridView _schemeGridView;
        private Label _lblSelectedScheme;
        private Label _lblStatus;
        private Label _lblDiabolicalRating;
        private Label _lblSuccessLikelihood;
        private ProgressBar _budgetProgressBar;
        private Label _lblBudgetStatus;
        private Label _lblDeadlineStatus;
        private Label _lblResourceStatus;
        private Button _btnNew;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnAssignResources;
        private Button _btnActivate;
        private Button _btnComplete;
        private Button _btnFail;
        private Button _btnPause;

        private EvilScheme _selectedScheme;

        public SchemeManagementForm(IRepository repository, IEvilSchemeService evilSchemeService)
        {
            _repository = repository;
            _evilSchemeService = evilSchemeService;
            InitializeComponent();
            
            // Initialize the service's schemes cache
            ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
            
            LoadSchemes();
        }

        private void InitializeComponent()
        {
            this.Text = "Evil Scheme Management";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Main layout: Split container with grid on left, details on right
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 500,
                Orientation = Orientation.Vertical
            };
            this.Controls.Add(splitContainer);

            // LEFT PANEL: Scheme List Grid
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            splitContainer.Panel1.Controls.Add(pnlLeft);

            var lblSchemeList = new Label
            {
                Text = "Evil Schemes",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlLeft.Controls.Add(lblSchemeList);

            _schemeGridView = new DataGridView
            {
                Location = new Point(0, 30),
                Size = new Size(480, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _schemeGridView.Columns.Add("SchemeId", "ID");
            _schemeGridView.Columns.Add("Name", "Scheme Name");
            _schemeGridView.Columns.Add("Status", "Status");
            _schemeGridView.Columns.Add("Success", "Success %");
            _schemeGridView.Columns.Add("BudgetStatus", "Budget");
            _schemeGridView.Columns.Add("DaysToDeadline", "Days");
            _schemeGridView.SelectionChanged += SchemeGridView_SelectionChanged;
            _schemeGridView.CellFormatting += SchemeGridView_CellFormatting;
            pnlLeft.Controls.Add(_schemeGridView);

            // Action buttons below grid
            var pnlButtons = new FlowLayoutPanel
            {
                Location = new Point(0, 540),
                Size = new Size(480, 40),
                AutoSize = false,
                WrapContents = true
            };

            _btnNew = CreateButton("New", 70);
            _btnEdit = CreateButton("Edit", 70);
            _btnDelete = CreateButton("Delete", 70);
            _btnAssignResources = CreateButton("Assign", 70);

            pnlButtons.Controls.AddRange(new Control[] { _btnNew, _btnEdit, _btnDelete, _btnAssignResources });
            pnlLeft.Controls.Add(pnlButtons);

            // RIGHT PANEL: Details
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoScroll = true
            };
            splitContainer.Panel2.Controls.Add(pnlRight);

            int yPos = 10;

            // Selected Scheme Header
            _lblSelectedScheme = new Label
            {
                Text = "No scheme selected",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos),
                ForeColor = Color.DarkBlue
            };
            pnlRight.Controls.Add(_lblSelectedScheme);
            yPos += 35;

            // Status Section
            var lblStatusLabel = new Label
            {
                Text = "Status:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos)
            };
            pnlRight.Controls.Add(lblStatusLabel);

            _lblStatus = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(100, yPos)
            };
            pnlRight.Controls.Add(_lblStatus);
            yPos += 30;

            // Diabolical Rating
            var lblRatingLabel = new Label
            {
                Text = "Diabolical Rating:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos)
            };
            pnlRight.Controls.Add(lblRatingLabel);

            _lblDiabolicalRating = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(150, yPos)
            };
            pnlRight.Controls.Add(_lblDiabolicalRating);
            yPos += 30;

            // Success Likelihood Section
            var lblSuccessLabel = new Label
            {
                Text = "Success Likelihood:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos)
            };
            pnlRight.Controls.Add(lblSuccessLabel);

            _lblSuccessLikelihood = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(150, yPos)
            };
            pnlRight.Controls.Add(_lblSuccessLikelihood);
            yPos += 30;

            // Budget Section Header
            var lblBudgetHeader = new Label
            {
                Text = "Budget",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 330,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlRight.Controls.Add(lblBudgetHeader);
            yPos += 30;

            // Budget Progress Bar
            _budgetProgressBar = new ProgressBar
            {
                Location = new Point(10, yPos),
                Size = new Size(330, 30),
                Minimum = 0,
                Maximum = 100
            };
            pnlRight.Controls.Add(_budgetProgressBar);
            yPos += 40;

            // Budget Status
            _lblBudgetStatus = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(10, yPos)
            };
            pnlRight.Controls.Add(_lblBudgetStatus);
            yPos += 30;

            // Deadline Section Header
            var lblDeadlineHeader = new Label
            {
                Text = "Deadline",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 330,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlRight.Controls.Add(lblDeadlineHeader);
            yPos += 30;

            _lblDeadlineStatus = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(10, yPos)
            };
            pnlRight.Controls.Add(_lblDeadlineStatus);
            yPos += 30;

            // Resources Section Header
            var lblResourceHeader = new Label
            {
                Text = "Resources",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 330,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlRight.Controls.Add(lblResourceHeader);
            yPos += 30;

            _lblResourceStatus = new Label
            {
                Text = "—",
                Font = new Font("Arial", 10),
                Location = new Point(10, yPos),
                AutoSize = true,
                MaximumSize = new Size(330, 200)
            };
            pnlRight.Controls.Add(_lblResourceStatus);
            yPos += 80;

            // Status Transition Buttons (bottom of details)
            var pnlStatusButtons = new FlowLayoutPanel
            {
                Location = new Point(10, yPos),
                Size = new Size(330, 50),
                WrapContents = true
            };

            _btnActivate = CreateButton("Activate", 70);
            _btnComplete = CreateButton("Complete", 70);
            _btnFail = CreateButton("Fail", 50);
            _btnPause = CreateButton("Pause", 60);

            pnlStatusButtons.Controls.AddRange(new Control[] { _btnActivate, _btnComplete, _btnFail, _btnPause });
            pnlRight.Controls.Add(pnlStatusButtons);

            // Initialize button event handlers
            InitializeButtonHandlers();
        }

        private Button CreateButton(string text, int width)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                Margin = new Padding(5, 5, 5, 5),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
        }

        private void LoadSchemes()
        {
            _schemeGridView.Rows.Clear();

            var schemes = _repository.GetAllSchemes();
            foreach (var scheme in schemes)
            {
                int successLikelihood = _evilSchemeService.CalculateSuccessLikelihood(scheme.SchemeId);
                var (budgetStatus, _) = _evilSchemeService.ValidateBudgetStatus(scheme.SchemeId);

                int daysToDeadline = (int)(scheme.TargetCompletionDate - DateTime.Now).TotalDays;

                _schemeGridView.Rows.Add(
                    scheme.SchemeId,
                    scheme.Name,
                    scheme.Status,
                    successLikelihood + "%",
                    budgetStatus,
                    daysToDeadline
                );
            }
        }

        private void SchemeGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_schemeGridView.SelectedRows.Count == 0)
            {
                _selectedScheme = null;
                ClearDetailsPanel();
                return;
            }

            int schemeId = (int)_schemeGridView.SelectedRows[0].Cells["SchemeId"].Value;
            _selectedScheme = _repository.GetSchemeById(schemeId);

            if (_selectedScheme != null)
            {
                UpdateDetailsPanel();
            }
        }

        private void UpdateDetailsPanel()
        {
            _lblSelectedScheme.Text = _selectedScheme.Name;
            _lblStatus.Text = _selectedScheme.Status;
            _lblDiabolicalRating.Text = _selectedScheme.DiabolicalRating + "/10";

            // Success Likelihood
            int success = _evilSchemeService.CalculateSuccessLikelihood(_selectedScheme.SchemeId);
            _lblSuccessLikelihood.Text = success + "% (";
            Color successColor = success >= 70 ? Color.Green : (success >= 40 ? Color.Orange : Color.Red);
            _lblSuccessLikelihood.ForeColor = successColor;

            // Budget Status
            var (budgetStatus, allowNewAssignments) = _evilSchemeService.ValidateBudgetStatus(_selectedScheme.SchemeId);
            decimal budgetPercent = (_selectedScheme.CurrentSpending / _selectedScheme.Budget) * 100;
            _budgetProgressBar.Value = (int)Math.Min(budgetPercent, 100);
            _lblBudgetStatus.Text = $"{budgetStatus}: {_selectedScheme.CurrentSpending:C} / {_selectedScheme.Budget:C}";

            Color budgetColor = budgetStatus.Contains("Over") ? Color.Red :
                               budgetStatus.Contains("Approaching") ? Color.Orange :
                               Color.Green;
            _lblBudgetStatus.ForeColor = budgetColor;

            // Deadline Status
            int daysToDeadline = (int)((_selectedScheme.TargetCompletionDate - DateTime.Now).TotalDays);
            string deadlineText;
            Color deadlineColor;

            if (daysToDeadline < 0)
            {
                deadlineText = $"OVERDUE ({Math.Abs(daysToDeadline)} days ago)";
                deadlineColor = Color.Red;
            }
            else if (daysToDeadline <= 7)
            {
                deadlineText = $"URGENT - {daysToDeadline} days remaining";
                deadlineColor = Color.Red;
            }
            else if (daysToDeadline <= 30)
            {
                deadlineText = $"Due Soon - {daysToDeadline} days remaining";
                deadlineColor = Color.Orange;
            }
            else
            {
                deadlineText = $"On Track - {daysToDeadline} days remaining";
                deadlineColor = Color.Green;
            }

            _lblDeadlineStatus.Text = deadlineText;
            _lblDeadlineStatus.ForeColor = deadlineColor;

            // Resources
            var assignedMinions = _repository.GetAllMinions()
                .Where(m => m.CurrentSchemeId == _selectedScheme.SchemeId).Count();
            var assignedEquipment = _repository.GetAllEquipment()
                .Where(e => e.AssignedToSchemeId == _selectedScheme.SchemeId).Count();

            var (reqMinions, reqEquipment, requiresDoomsday) = _evilSchemeService.GetResourceRequirements(_selectedScheme.DiabolicalRating);

            string resourceText = $"Minions: {assignedMinions} / {reqMinions}\n" +
                                 $"Equipment: {assignedEquipment} / {reqEquipment}\n" +
                                 $"Doomsday Device: {(requiresDoomsday ? "REQUIRED" : "Not required")}";

            _lblResourceStatus.Text = resourceText;
        }

        private void ClearDetailsPanel()
        {
            _lblSelectedScheme.Text = "No scheme selected";
            _lblStatus.Text = "—";
            _lblDiabolicalRating.Text = "—";
            _lblSuccessLikelihood.Text = "—";
            _lblBudgetStatus.Text = "—";
            _lblDeadlineStatus.Text = "—";
            _lblResourceStatus.Text = "—";
            _budgetProgressBar.Value = 0;
        }

        private void SchemeGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == _schemeGridView.Columns["Status"].Index)
            {
                string status = e.Value?.ToString() ?? "";
                e.CellStyle.BackColor = status switch
                {
                    "Active" => Color.FromArgb(200, 220, 255),
                    "Completed" => Color.FromArgb(200, 255, 200),
                    "Failed" => Color.FromArgb(255, 200, 200),
                    "On Hold" => Color.FromArgb(255, 255, 150),
                    _ => Color.White
                };
                e.CellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            }

            if (e.ColumnIndex == _schemeGridView.Columns["BudgetStatus"].Index)
            {
                string budgetStatus = e.Value?.ToString() ?? "";
                e.CellStyle.ForeColor = budgetStatus.Contains("Over") ? Color.Red :
                                       budgetStatus.Contains("Approaching") ? Color.Orange :
                                       Color.Green;
            }
        }

        // Event Handlers for Action Buttons
        private void InitializeButtonHandlers()
        {
            _btnNew.Click += BtnNew_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnAssignResources.Click += BtnAssignResources_Click;
            _btnActivate.Click += BtnActivate_Click;
            _btnComplete.Click += BtnComplete_Click;
            _btnFail.Click += BtnFail_Click;
            _btnPause.Click += BtnPause_Click;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var dialog = new SchemeEditDialog(_repository, _evilSchemeService))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Reinitialize the schemes cache before loading
                    ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
                    LoadSchemes();
                    ClearDetailsPanel();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
            {
                MessageBox.Show("Please select a scheme to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SchemeEditDialog(_repository, _evilSchemeService, _selectedScheme))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Reinitialize the schemes cache before loading
                    ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
                    LoadSchemes();
                    UpdateDetailsPanel();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
            {
                MessageBox.Show("Please select a scheme to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedScheme.Name}'? This cannot be undone.",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _repository.DeleteScheme(_selectedScheme.SchemeId);
                LoadSchemes();
                ClearDetailsPanel();
            }
        }

        private void BtnAssignResources_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
            {
                MessageBox.Show("Please select a scheme to assign resources to.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show a submenu to choose between minions and equipment
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Assign Minions", null, (s, args) => ShowAssignMinionsDialog());
            contextMenu.Items.Add("Assign Equipment", null, (s, args) => ShowAssignEquipmentDialog());

            contextMenu.Show(_btnAssignResources, new Point(0, _btnAssignResources.Height));
        }

        private void ShowAssignMinionsDialog()
        {
            using (var dialog = new AssignMinionsDialog(_repository, _evilSchemeService, _selectedScheme))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh the scheme data and update display
                    _selectedScheme = _repository.GetSchemeById(_selectedScheme.SchemeId);
                    ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
                    LoadSchemes();
                    UpdateDetailsPanel();
                }
            }
        }

        private void ShowAssignEquipmentDialog()
        {
            using (var dialog = new AssignEquipmentDialog(_repository, _evilSchemeService, _selectedScheme))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh the scheme data and update display
                    _selectedScheme = _repository.GetSchemeById(_selectedScheme.SchemeId);
                    ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
                    LoadSchemes();
                    UpdateDetailsPanel();
                }
            }
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
                return;

            var (canTransition, errors) = _evilSchemeService.CanTransitionToStatus(_selectedScheme.SchemeId, "Active");

            if (!canTransition)
            {
                MessageBox.Show(
                    "Cannot activate scheme:\n" + string.Join("\n", errors),
                    "Transition Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _selectedScheme.Status = "Active";
            _repository.UpdateScheme(_selectedScheme);
            ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
            LoadSchemes();
            UpdateDetailsPanel();
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
                return;

            var (canTransition, errors) = _evilSchemeService.CanTransitionToStatus(_selectedScheme.SchemeId, "Completed");

            if (!canTransition)
            {
                MessageBox.Show(
                    "Cannot complete scheme:\n" + string.Join("\n", errors),
                    "Transition Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _selectedScheme.Status = "Completed";
            _repository.UpdateScheme(_selectedScheme);
            LoadSchemes();
            UpdateDetailsPanel();
        }

        private void BtnFail_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to mark this scheme as Failed?",
                "Confirm Failure",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _selectedScheme.Status = "Failed";
                _repository.UpdateScheme(_selectedScheme);
                ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
                LoadSchemes();
                UpdateDetailsPanel();
            }
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (_selectedScheme == null)
                return;

            _selectedScheme.Status = "On Hold";
            _repository.UpdateScheme(_selectedScheme);
            ((EvilSchemeService)_evilSchemeService).InitializeSchemes();
            LoadSchemes();
            UpdateDetailsPanel();
        }
    }
}
