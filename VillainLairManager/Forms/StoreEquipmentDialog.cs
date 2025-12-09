using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;

namespace VillainLairManager.Forms
{
    public partial class StoreEquipmentDialog : Form
    {
        private readonly IRepository _repository;
        private readonly SecretBase _base;
        private ListBox _lstAvailableEquipment;
        private ListBox _lstStoredEquipment;
        private Button _btnAdd;
        private Button _btnRemove;
        private Label _lblEquipmentCount;

        public StoreEquipmentDialog(IRepository repository, SecretBase base_)
        {
            _repository = repository;
            _base = base_;
            InitializeComponent();
            LoadEquipment();
        }

        private void InitializeComponent()
        {
            this.Text = $"Store Equipment at {_base.Name}";
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
                Text = "Equipment Management",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlMain.Controls.Add(lblTitle);

            // Available Equipment (left)
            var lblAvailable = new Label
            {
                Text = "Available Equipment (≥50% condition):",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 35)
            };
            pnlMain.Controls.Add(lblAvailable);

            _lstAvailableEquipment = new ListBox
            {
                Location = new Point(0, 60),
                Size = new Size(240, 320),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstAvailableEquipment);

            // Buttons in middle
            _btnAdd = new Button
            {
                Text = "→ Store",
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
                Text = "← Unstore",
                Location = new Point(250, 160),
                Size = new Size(80, 35),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            _btnRemove.Click += BtnRemove_Click;
            pnlMain.Controls.Add(_btnRemove);

            // Stored Equipment (right)
            var lblStored = new Label
            {
                Text = "Stored at This Base:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(345, 35)
            };
            pnlMain.Controls.Add(lblStored);

            _lstStoredEquipment = new ListBox
            {
                Location = new Point(345, 60),
                Size = new Size(240, 320),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = SelectionMode.One
            };
            pnlMain.Controls.Add(_lstStoredEquipment);

            // Equipment count info
            _lblEquipmentCount = new Label
            {
                Text = "Equipment stored: 0",
                Font = new Font("Arial", 9),
                AutoSize = true,
                Location = new Point(0, 390)
            };
            pnlMain.Controls.Add(_lblEquipmentCount);

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

        private void LoadEquipment()
        {
            var allEquipment = _repository.GetAllEquipment();

            // Available: not at this base, ≥50% condition
            var available = allEquipment
                .Where(e => e.StoredAtBaseId != _base.BaseId && e.Condition >= 50)
                .OrderBy(e => e.Name)
                .ToList();

            _lstAvailableEquipment.Items.Clear();
            foreach (var equipment in available)
            {
                _lstAvailableEquipment.Items.Add(
                    new EquipmentListItem { Equipment = equipment, DisplayText = $"{equipment.Name} ({equipment.Condition}%)" }
                );
            }

            // Stored: at this base
            var stored = allEquipment
                .Where(e => e.StoredAtBaseId == _base.BaseId)
                .OrderBy(e => e.Name)
                .ToList();

            _lstStoredEquipment.Items.Clear();
            foreach (var equipment in stored)
            {
                _lstStoredEquipment.Items.Add(
                    new EquipmentListItem { Equipment = equipment, DisplayText = $"{equipment.Name} ({equipment.Condition}%)" }
                );
            }

            _lblEquipmentCount.Text = $"Equipment stored: {stored.Count}";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_lstAvailableEquipment.SelectedItem is not EquipmentListItem item)
            {
                MessageBox.Show("Please select equipment to store", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            item.Equipment.StoredAtBaseId = _base.BaseId;
            _repository.UpdateEquipment(item.Equipment);
            LoadEquipment();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_lstStoredEquipment.SelectedItem is not EquipmentListItem item)
            {
                MessageBox.Show("Please select equipment to unstore", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.Equipment.Condition < 50)
            {
                var result = MessageBox.Show(
                    $"Warning: {item.Equipment.Name} is in poor condition ({item.Equipment.Condition}%). Unstore anyway?",
                    "Low Condition",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;
            }

            item.Equipment.StoredAtBaseId = null;
            _repository.UpdateEquipment(item.Equipment);
            LoadEquipment();
        }

        private class EquipmentListItem
        {
            public Equipment Equipment { get; set; }
            public string DisplayText { get; set; }

            public override string ToString() => DisplayText;
        }
    }
}
