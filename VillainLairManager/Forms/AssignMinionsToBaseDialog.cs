using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;

namespace VillainLairManager.Forms
{
    public partial class AssignMinionsToBaseDialog : Form
    {
        private readonly IRepository _repository;
        private readonly SecretBase _base;
        private ListBox _lstAvailableMinions;
        private ListBox _lstAssignedMinions;
        private Button _btnAdd;
        private Button _btnRemove;
        private Label _lblOccupancy;

        public AssignMinionsToBaseDialog(IRepository repository, SecretBase base_)
        {
            _repository = repository;
            _base = base_;
            InitializeComponent();
            LoadMinions();
        }

        private void InitializeComponent()
        {
            this.Text = $"Assign Minions to {_base.Name}";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
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

            var lblTitle = new Label
            {
                Text = "Minion Assignment",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlMain.Controls.Add(lblTitle);

            // Available Minions (left)
            var lblAvailable = new Label
            {
                Text = "Available Minions:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 35)
            };
            pnlMain.Controls.Add(lblAvailable);

            _lstAvailableMinions = new ListBox
            {
                Location = new Point(0, 60),
                Size = new Size(240, 320),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAvailableMinions);

            // Buttons in middle
            _btnAdd = new Button
            {
                Text = "→ Assign",
                Location = new Point(250, 120),
                Size = new Size(80, 35),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            _btnAdd.Click += BtnAdd_Click;
            pnlMain.Controls.Add(_btnAdd);

            _btnRemove = new Button
            {
                Text = "← Unassign",
                Location = new Point(250, 160),
                Size = new Size(80, 35),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            _btnRemove.Click += BtnRemove_Click;
            pnlMain.Controls.Add(_btnRemove);

            // Assigned Minions (right)
            var lblAssigned = new Label
            {
                Text = "Assigned to This Base:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(345, 35)
            };
            pnlMain.Controls.Add(lblAssigned);

            _lstAssignedMinions = new ListBox
            {
                Location = new Point(345, 60),
                Size = new Size(240, 320),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAssignedMinions);

            // Occupancy info
            _lblOccupancy = new Label
            {
                Text = $"Occupancy: 0 / {_base.Capacity}",
                Font = new Font("Arial", 9),
                AutoSize = true,
                Location = new Point(0, 390)
            };
            pnlMain.Controls.Add(_lblOccupancy);

            // Buttons at bottom
            var btnDone = new Button
            {
                Text = "Done",
                Location = new Point(490, 420),
                Size = new Size(90, 35),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            pnlMain.Controls.Add(btnDone);
        }

        private void LoadMinions()
        {
            var allMinions = _repository.GetAllMinions();

            // Available: not at this base
            var available = allMinions
                .Where(m => m.CurrentBaseId != _base.BaseId)
                .OrderBy(m => m.Name)
                .ToList();

            _lstAvailableMinions.Items.Clear();
            foreach (var minion in available)
            {
                _lstAvailableMinions.Items.Add(
                    new MinionListItem { Minion = minion, DisplayText = $"{minion.Name} ({minion.Specialty})" }
                );
            }

            // Assigned: at this base
            var assigned = allMinions
                .Where(m => m.CurrentBaseId == _base.BaseId)
                .OrderBy(m => m.Name)
                .ToList();

            _lstAssignedMinions.Items.Clear();
            foreach (var minion in assigned)
            {
                _lstAssignedMinions.Items.Add(
                    new MinionListItem { Minion = minion, DisplayText = $"{minion.Name} ({minion.Specialty})" }
                );
            }

            _lblOccupancy.Text = $"Occupancy: {assigned.Count} / {_base.Capacity}";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_lstAvailableMinions.SelectedItem is not MinionListItem item)
            {
                MessageBox.Show("Please select a minion to assign", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var assigned = _repository.GetAllMinions().Count(m => m.CurrentBaseId == _base.BaseId);
            if (assigned >= _base.Capacity)
            {
                MessageBox.Show($"Base is at full capacity ({_base.Capacity} minions)", "Capacity Full", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            item.Minion.CurrentBaseId = _base.BaseId;
            _repository.UpdateMinion(item.Minion);
            LoadMinions();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_lstAssignedMinions.SelectedItem is not MinionListItem item)
            {
                MessageBox.Show("Please select a minion to unassign", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            item.Minion.CurrentBaseId = null;
            _repository.UpdateMinion(item.Minion);
            LoadMinions();
        }

        private class MinionListItem
        {
            public Minion Minion { get; set; }
            public string DisplayText { get; set; }

            public override string ToString() => DisplayText;
        }
    }
}
