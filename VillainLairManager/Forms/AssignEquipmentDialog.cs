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
    /// Dialog for assigning equipment to a scheme with doomsday device requirement validation
    /// </summary>
    public class AssignEquipmentDialog : Form
    {
        private readonly IRepository _repository;
        private readonly IEvilSchemeService _evilSchemeService;
        private readonly EvilScheme _scheme;

        private ListBox _lstAvailableEquipment;
        private ListBox _lstAssignedEquipment;
        private Button _btnAddEquipment;
        private Button _btnRemoveEquipment;
        private Label _lblResourceStatus;
        private Label _lblDoomsdayStatus;
        private Button _btnSave;
        private Button _btnCancel;

        public AssignEquipmentDialog(IRepository repository, IEvilSchemeService evilSchemeService, EvilScheme scheme)
        {
            _repository = repository;
            _evilSchemeService = evilSchemeService;
            _scheme = scheme;

            InitializeComponent();
            LoadEquipment();
        }

        private void InitializeComponent()
        {
            this.Text = $"Assign Equipment to '{_scheme.Name}'";
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
                Text = "Equipment Assignment",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblTitle);
            yPos += 35;

            // Available Equipment Section
            var lblAvailable = new Label
            {
                Text = "Available Equipment (Condition ≥ 50%)",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblAvailable);
            yPos += 25;

            _lstAvailableEquipment = new ListBox
            {
                Location = new Point(10, yPos),
                Size = new Size(250, 200),
                Font = new Font("Arial", 9),
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAvailableEquipment);

            // Arrow buttons
            var pnlArrows = new Panel
            {
                Location = new Point(270, yPos + 50),
                Size = new Size(100, 100),
                BackColor = Color.White
            };

            _btnAddEquipment = new Button
            {
                Text = "➜ Add",
                Location = new Point(10, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(100, 200, 100),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
            _btnAddEquipment.Click += BtnAddEquipment_Click;
            pnlArrows.Controls.Add(_btnAddEquipment);

            _btnRemoveEquipment = new Button
            {
                Text = "Remove",
                Location = new Point(10, 50),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(200, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Arial", 9)
            };
            _btnRemoveEquipment.Click += BtnRemoveEquipment_Click;
            pnlArrows.Controls.Add(_btnRemoveEquipment);

            pnlMain.Controls.Add(pnlArrows);

            // Assigned Equipment Section
            var lblAssigned = new Label
            {
                Text = "Assigned to This Scheme",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(390, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblAssigned);

            _lstAssignedEquipment = new ListBox
            {
                Location = new Point(390, yPos + 25),
                Size = new Size(250, 200),
                Font = new Font("Arial", 9),
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAssignedEquipment);

            yPos += 230;

            // Info Section
            var lblInfoHeader = new Label
            {
                Text = "Requirements & Status",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, yPos),
                BackColor = Color.LightGray,
                Width = 630,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblInfoHeader);
            yPos += 30;

            // Resource Status
            _lblResourceStatus = new Label
            {
                Text = "Resources: —",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPos),
                AutoSize = true,
                MaximumSize = new Size(620, 100)
            };
            pnlMain.Controls.Add(_lblResourceStatus);
            yPos += 60;

            // Doomsday Device Status
            _lblDoomsdayStatus = new Label
            {
                Text = "Doomsday Device: —",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            pnlMain.Controls.Add(_lblDoomsdayStatus);
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

        private void LoadEquipment()
        {
            _lstAvailableEquipment.Items.Clear();
            _lstAssignedEquipment.Items.Clear();

            var allEquipment = _repository.GetAllEquipment();

            foreach (var equipment in allEquipment)
            {
                var displayText = $"{equipment.Name} ({equipment.Category}, {equipment.Condition}% condition)";

                if (equipment.AssignedToSchemeId == _scheme.SchemeId)
                {
                    _lstAssignedEquipment.Items.Add(new EquipmentDisplayItem { Equipment = equipment, DisplayText = displayText });
                }
                else if ((equipment.AssignedToSchemeId == 0 || equipment.AssignedToSchemeId == null) && 
                         equipment.Condition >= 50)
                {
                    _lstAvailableEquipment.Items.Add(new EquipmentDisplayItem { Equipment = equipment, DisplayText = displayText });
                }
            }

            UpdatePreview();
        }

        private void BtnAddEquipment_Click(object sender, EventArgs e)
        {
            if (_lstAvailableEquipment.SelectedItem is EquipmentDisplayItem item)
            {
                _lstAvailableEquipment.Items.Remove(item);
                _lstAssignedEquipment.Items.Add(item);
                UpdatePreview();
            }
        }

        private void BtnRemoveEquipment_Click(object sender, EventArgs e)
        {
            if (_lstAssignedEquipment.SelectedItem is EquipmentDisplayItem item)
            {
                _lstAssignedEquipment.Items.Remove(item);
                _lstAvailableEquipment.Items.Add(item);
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            int assignedCount = _lstAssignedEquipment.Items.Count;
            var (reqMinions, reqEquipment, requiresDoomsday) = _evilSchemeService.GetResourceRequirements(_scheme.DiabolicalRating);

            // Check for doomsday device
            bool hasDoomsday = _lstAssignedEquipment.Items.Cast<EquipmentDisplayItem>()
                .Any(item => item.Equipment.Category.ToLower().Contains("doomsday"));

            // Resource Status
            string resourceText = $"Equipment: {assignedCount} assigned (requires {reqEquipment})\n";
            Color resourceColor = assignedCount >= reqEquipment ? Color.Green : Color.Orange;
            
            _lblResourceStatus.Text = resourceText;
            _lblResourceStatus.ForeColor = resourceColor;

            // Doomsday Device Status
            if (requiresDoomsday)
            {
                _lblDoomsdayStatus.Text = hasDoomsday ? "✓ Doomsday Device: ASSIGNED" : "✗ Doomsday Device: REQUIRED but not assigned";
                _lblDoomsdayStatus.ForeColor = hasDoomsday ? Color.Green : Color.Red;
            }
            else
            {
                _lblDoomsdayStatus.Text = "Doomsday Device: Not required for this scheme";
                _lblDoomsdayStatus.ForeColor = Color.Gray;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate doomsday device requirement
            var (reqMinions, reqEquipment, requiresDoomsday) = _evilSchemeService.GetResourceRequirements(_scheme.DiabolicalRating);

            if (requiresDoomsday)
            {
                bool hasDoomsday = _lstAssignedEquipment.Items.Cast<EquipmentDisplayItem>()
                    .Any(item => item.Equipment.Category.ToLower().Contains("doomsday"));

                if (!hasDoomsday)
                {
                    MessageBox.Show(
                        "This scheme requires a Doomsday Device for its diabolical rating.\n" +
                        "Please assign one before saving.",
                        "Missing Doomsday Device",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            // Update all equipment's AssignedToSchemeId
            var allEquipment = _repository.GetAllEquipment();

            // First, unassign any equipment no longer in the list
            foreach (var equipment in allEquipment.Where(e => e.AssignedToSchemeId == _scheme.SchemeId))
            {
                bool stillAssigned = _lstAssignedEquipment.Items.Cast<EquipmentDisplayItem>()
                    .Any(item => item.Equipment.EquipmentId == equipment.EquipmentId);

                if (!stillAssigned)
                {
                    equipment.AssignedToSchemeId = 0;
                    _repository.UpdateEquipment(equipment);
                }
            }

            // Then, assign newly added equipment
            foreach (EquipmentDisplayItem item in _lstAssignedEquipment.Items)
            {
                item.Equipment.AssignedToSchemeId = _scheme.SchemeId;
                _repository.UpdateEquipment(item.Equipment);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Helper class to display equipment info while keeping reference to equipment object
        private class EquipmentDisplayItem
        {
            public Equipment Equipment { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}
