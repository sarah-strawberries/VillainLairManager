using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class BaseEditDialog : Form
    {
        private readonly IRepository _repository;
        private readonly ISecretBaseService _baseService;
        private SecretBase _base;
        private bool _isNewBase;

        private TextBox _txtName;
        private TextBox _txtLocation;
        private NumericUpDown _numCapacity;
        private NumericUpDown _numSecurityLevel;
        private NumericUpDown _numMaintenanceCost;
        private CheckBox _chkDoomsdayDevice;
        private CheckBox _chkIsDiscovered;
        private DateTimePicker _dtpLastInspection;
        private Label _lblValidationMessage;

        public BaseEditDialog(IRepository repository, ISecretBaseService baseService, SecretBase existingBase)
        {
            _repository = repository;
            _baseService = baseService;
            _base = existingBase ?? new SecretBase();
            _isNewBase = (existingBase == null);

            InitializeComponent();
            if (!_isNewBase)
            {
                LoadFormData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _isNewBase ? "Create New Secret Base" : "Edit Secret Base";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            this.Controls.Add(pnlMain);

            int yPos = 10;

            // Title
            var lblTitle = new Label
            {
                Text = _isNewBase ? "New Secret Base" : $"Edit: {_base.Name}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            pnlMain.Controls.Add(lblTitle);
            yPos += 40;

            // Name
            var lblName = new Label { Text = "Base Name:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblName);
            _txtName = new TextBox { Location = new Point(120, yPos), Size = new Size(300, 25) };
            pnlMain.Controls.Add(_txtName);
            yPos += 35;

            // Location
            var lblLocation = new Label { Text = "Location:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblLocation);
            _txtLocation = new TextBox { Location = new Point(120, yPos), Size = new Size(300, 25) };
            pnlMain.Controls.Add(_txtLocation);
            yPos += 35;

            // Capacity
            var lblCapacity = new Label { Text = "Capacity:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblCapacity);
            _numCapacity = new NumericUpDown { Location = new Point(120, yPos), Size = new Size(100, 25), Minimum = 1, Maximum = 200, Value = 50 };
            pnlMain.Controls.Add(_numCapacity);
            yPos += 35;

            // Security Level
            var lblSecurity = new Label { Text = "Security Level:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblSecurity);
            _numSecurityLevel = new NumericUpDown { Location = new Point(120, yPos), Size = new Size(100, 25), Minimum = 1, Maximum = 10, Value = 5 };
            pnlMain.Controls.Add(_numSecurityLevel);
            yPos += 35;

            // Monthly Maintenance Cost
            var lblMaintenance = new Label { Text = "Monthly Cost:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblMaintenance);
            _numMaintenanceCost = new NumericUpDown { Location = new Point(120, yPos), Size = new Size(150, 25), Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Value = 5000 };
            pnlMain.Controls.Add(_numMaintenanceCost);
            yPos += 35;

            // Doomsday Device Checkbox
            _chkDoomsdayDevice = new CheckBox { Text = "Has Doomsday Device", Location = new Point(0, yPos), AutoSize = true };
            pnlMain.Controls.Add(_chkDoomsdayDevice);
            yPos += 30;

            // Discovery Checkbox
            _chkIsDiscovered = new CheckBox { Text = "Base Discovered", Location = new Point(0, yPos), AutoSize = true };
            pnlMain.Controls.Add(_chkIsDiscovered);
            yPos += 30;

            // Last Inspection Date
            var lblInspection = new Label { Text = "Last Inspection:", AutoSize = true, Location = new Point(0, yPos) };
            pnlMain.Controls.Add(lblInspection);
            _dtpLastInspection = new DateTimePicker { Location = new Point(120, yPos), Size = new Size(150, 25) };
            pnlMain.Controls.Add(_dtpLastInspection);
            yPos += 35;

            // Validation Message
            _lblValidationMessage = new Label
            {
                Text = "",
                Font = new Font("Arial", 9),
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(0, yPos),
                MaximumSize = new Size(400, 0),
                Visible = false
            };
            pnlMain.Controls.Add(_lblValidationMessage);
            yPos += 60;

            // Button Panel
            var pnlButtons = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(460, 50),
                Dock = DockStyle.Bottom
            };
            pnlMain.Controls.Add(pnlButtons);

            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(200, 10),
                Size = new Size(100, 35),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;
            pnlButtons.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(310, 10),
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            pnlButtons.Controls.Add(btnCancel);
        }

        private void LoadFormData()
        {
            _txtName.Text = _base.Name;
            _txtLocation.Text = _base.Location;
            _numCapacity.Value = _base.Capacity;
            _numSecurityLevel.Value = _base.SecurityLevel;
            _numMaintenanceCost.Value = _base.MonthlyMaintenanceCost;
            _chkDoomsdayDevice.Checked = _base.HasDoomsdayDevice;
            _chkIsDiscovered.Checked = _base.IsDiscovered;
            if (_base.LastInspectionDate.HasValue)
            {
                _dtpLastInspection.Value = _base.LastInspectionDate.Value;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_txtName.Text))
                errors.Add("• Base name is required");

            if (string.IsNullOrWhiteSpace(_txtLocation.Text))
                errors.Add("• Location is required");

            if (_numCapacity.Value < 1)
                errors.Add("• Capacity must be at least 1");

            if (_numMaintenanceCost.Value < 0)
                errors.Add("• Monthly cost cannot be negative");

            if (errors.Count > 0)
            {
                _lblValidationMessage.Text = "Validation Errors:\n" + string.Join("\n", errors);
                _lblValidationMessage.Visible = true;
                this.DialogResult = DialogResult.None;
                return;
            }

            // Update base object
            _base.Name = _txtName.Text;
            _base.Location = _txtLocation.Text;
            _base.Capacity = (int)_numCapacity.Value;
            _base.SecurityLevel = (int)_numSecurityLevel.Value;
            _base.MonthlyMaintenanceCost = _numMaintenanceCost.Value;
            _base.HasDoomsdayDevice = _chkDoomsdayDevice.Checked;
            _base.IsDiscovered = _chkIsDiscovered.Checked;
            _base.LastInspectionDate = _dtpLastInspection.Value;

            // Save to database
            if (_isNewBase)
            {
                _repository.InsertBase(_base);
            }
            else
            {
                _repository.UpdateBase(_base);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
