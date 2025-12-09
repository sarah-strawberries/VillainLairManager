using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Dialog for assigning minions to a scheme with budget validation and cost preview
    /// </summary>
    public class AssignMinionsDialog : Form
    {
        private readonly IRepository _repository;
        private readonly IEvilSchemeService _evilSchemeService;
        private readonly EvilScheme _scheme;

        private ListBox _lstAvailableMinions;
        private ListBox _lstAssignedMinions;
        private Button _btnAddMinion;
        private Button _btnRemoveMinion;
        private Label _lblCostPreview;
        private Label _lblBudgetStatus;
        private Label _lblSuccessPreview;
        private Button _btnSave;
        private Button _btnCancel;

        public AssignMinionsDialog(IRepository repository, IEvilSchemeService evilSchemeService, EvilScheme scheme)
        {
            _repository = repository;
            _evilSchemeService = evilSchemeService;
            _scheme = scheme;

            InitializeComponent();
            LoadMinions();
        }

        private void InitializeComponent()
        {
            this.Text = $"Assign Minions to '{_scheme.Name}'";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Main panel
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            this.Controls.Add(pnlMain);

            int yPos = 10;

            // Title
            var lblTitle = new Label
            {
                Text = "Minion Assignment",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblTitle);
            yPos += 35;

            // Available Minions Section
            var lblAvailable = new Label
            {
                Text = "Available Minions",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblAvailable);
            yPos += 25;

            _lstAvailableMinions = new ListBox
            {
                Location = new Point(10, yPos),
                Size = new Size(250, 200),
                Font = new Font("Arial", 9),
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAvailableMinions);

            // Arrow buttons
            var pnlArrows = new Panel
            {
                Location = new Point(270, yPos + 50),
                Size = new Size(100, 100),
                BackColor = Color.White
            };

            _btnAddMinion = new Button
            {
                Text = "➜ Add",
                Location = new Point(10, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(100, 200, 100),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
            _btnAddMinion.Click += BtnAddMinion_Click;
            pnlArrows.Controls.Add(_btnAddMinion);

            _btnRemoveMinion = new Button
            {
                Text = "Remove",
                Location = new Point(10, 50),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(200, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
            _btnRemoveMinion.Click += BtnRemoveMinion_Click;
            pnlArrows.Controls.Add(_btnRemoveMinion);

            pnlMain.Controls.Add(pnlArrows);

            // Assigned Minions Section
            var lblAssigned = new Label
            {
                Text = "Assigned to This Scheme",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(390, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblAssigned);

            _lstAssignedMinions = new ListBox
            {
                Location = new Point(390, yPos + 25),
                Size = new Size(250, 200),
                Font = new Font("Arial", 9),
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAssignedMinions);

            yPos += 230;

            // Info Section
            var lblInfoHeader = new Label
            {
                Text = "Assignment Impact",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 630,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblInfoHeader);
            yPos += 30;

            // Cost Preview
            _lblCostPreview = new Label
            {
                Text = "Monthly Cost Impact: —",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(_lblCostPreview);
            yPos += 25;

            // Budget Status
            _lblBudgetStatus = new Label
            {
                Text = "Budget Status: —",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(_lblBudgetStatus);
            yPos += 25;

            // Success Preview
            _lblSuccessPreview = new Label
            {
                Text = "Projected Success: —",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(_lblSuccessPreview);
            yPos += 35;

            // Buttons
            var pnlButtons = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(630, 50),
                BackColor = Color.White
            };

            _btnSave = new Button
            {
                Text = "Save Assignments",
                Location = new Point(475, 10),
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
            _btnSave.Click += BtnSave_Click;
            pnlButtons.Controls.Add(_btnSave);

            _btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(560, 10),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                Font = new Font("Arial", 9),
                DialogResult = DialogResult.Cancel
            };
            pnlButtons.Controls.Add(_btnCancel);

            pnlMain.Controls.Add(pnlButtons);
        }

        private void LoadMinions()
        {
            _lstAvailableMinions.Items.Clear();
            _lstAssignedMinions.Items.Clear();

            var allMinions = _repository.GetAllMinions();

            foreach (var minion in allMinions)
            {
                var displayText = $"{minion.Name} (${minion.SalaryDemand}/mo, {minion.Specialty})";

                if (minion.CurrentSchemeId == _scheme.SchemeId)
                {
                    _lstAssignedMinions.Items.Add(new MinionDisplayItem { Minion = minion, DisplayText = displayText });
                }
                else if (minion.CurrentSchemeId == 0 || minion.CurrentSchemeId == null)
                {
                    _lstAvailableMinions.Items.Add(new MinionDisplayItem { Minion = minion, DisplayText = displayText });
                }
            }

            UpdatePreview();
        }

        private void BtnAddMinion_Click(object sender, EventArgs e)
        {
            if (_lstAvailableMinions.SelectedItem is MinionDisplayItem item)
            {
                var minion = item.Minion;

                // Check if assignment would exceed budget
                var (estimated, newTotal, wouldExceed) = _evilSchemeService.CalculateEstimatedSpending(_scheme.SchemeId, minion);

                if (wouldExceed)
                {
                    MessageBox.Show(
                        $"Cannot assign {minion.Name}. Assignment would exceed budget.\n" +
                        $"Current: {_scheme.CurrentSpending:C}\n" +
                        $"Would add: {estimated:C}\n" +
                        $"Budget: {_scheme.Budget:C}",
                        "Budget Exceeded",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Move minion
                _lstAvailableMinions.Items.Remove(item);
                _lstAssignedMinions.Items.Add(item);

                // Update scheme's spending
                _scheme.CurrentSpending = newTotal;

                UpdatePreview();
            }
        }

        private void BtnRemoveMinion_Click(object sender, EventArgs e)
        {
            if (_lstAssignedMinions.SelectedItem is MinionDisplayItem item)
            {
                var minion = item.Minion;

                // Calculate months remaining
                int monthsRemaining = (int)((_scheme.TargetCompletionDate - DateTime.Now).TotalDays / 30.44);
                if (monthsRemaining < 1) monthsRemaining = 1;

                decimal amountToRemove = minion.SalaryDemand * monthsRemaining;
                _scheme.CurrentSpending = Math.Max(0, _scheme.CurrentSpending - amountToRemove);

                // Move minion back
                _lstAssignedMinions.Items.Remove(item);
                _lstAvailableMinions.Items.Add(item);

                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            // Calculate total monthly cost of assigned minions
            decimal monthlyTotal = 0;
            foreach (MinionDisplayItem item in _lstAssignedMinions.Items)
            {
                monthlyTotal += item.Minion.SalaryDemand;
            }

            _lblCostPreview.Text = $"Total Monthly Cost: {monthlyTotal:C}";
            _lblCostPreview.ForeColor = monthlyTotal > 0 ? Color.DarkBlue : Color.Gray;

            // Budget status
            var (budgetStatus, _) = _evilSchemeService.ValidateBudgetStatus(_scheme.SchemeId);
            _lblBudgetStatus.Text = $"Budget Status: {budgetStatus}";

            Color budgetColor = budgetStatus.Contains("Over") ? Color.Red :
                               budgetStatus.Contains("Approaching") ? Color.Orange :
                               Color.Green;
            _lblBudgetStatus.ForeColor = budgetColor;

            // Success preview
            int assignedCount = _lstAssignedMinions.Items.Count;
            int success = _evilSchemeService.CalculateSuccessLikelihood(_scheme.SchemeId);
            _lblSuccessPreview.Text = $"Projected Success: {success}% ({assignedCount} minions assigned)";

            Color successColor = success >= 70 ? Color.Green : (success >= 40 ? Color.Orange : Color.Red);
            _lblSuccessPreview.ForeColor = successColor;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Update all minions' CurrentSchemeId
            var allMinions = _repository.GetAllMinions();

            // First, unassign any minions no longer in the list
            foreach (var minion in allMinions.Where(m => m.CurrentSchemeId == _scheme.SchemeId))
            {
                bool stillAssigned = _lstAssignedMinions.Items.Cast<MinionDisplayItem>()
                    .Any(item => item.Minion.MinionId == minion.MinionId);

                if (!stillAssigned)
                {
                    minion.CurrentSchemeId = 0;
                    _repository.UpdateMinion(minion);
                }
            }

            // Then, assign newly added minions
            foreach (MinionDisplayItem item in _lstAssignedMinions.Items)
            {
                item.Minion.CurrentSchemeId = _scheme.SchemeId;
                _repository.UpdateMinion(item.Minion);
            }

            // Update scheme spending in database
            _repository.UpdateScheme(_scheme);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Helper class to display minion info while keeping reference to minion object
        private class MinionDisplayItem
        {
            public Minion Minion { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}
