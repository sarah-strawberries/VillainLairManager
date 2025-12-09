using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class EquipmentInventoryForm : Form
    {
        private readonly IRepository _repository;
        private readonly IEquipmentService _equipmentService;
        private DataGridView dgvEquipment;
        private TextBox txtName;
        private ComboBox cmbCategory;
        private NumericUpDown numCondition;
        private NumericUpDown numPrice;
        private NumericUpDown numMaintenanceCost;
        private CheckBox chkSpecialist;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRepair;

        public EquipmentInventoryForm(IRepository repository, IEquipmentService equipmentService)
        {
            _repository = repository;
            _equipmentService = equipmentService;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Equipment Inventory";
            this.Size = new System.Drawing.Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Grid
            dgvEquipment = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(840, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvEquipment.SelectionChanged += DgvEquipment_SelectionChanged;
            this.Controls.Add(dgvEquipment);

            // Input Panel
            var pnlInputs = new Panel
            {
                Location = new Point(20, 440),
                Size = new Size(840, 150),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlInputs);

            // Inputs
            pnlInputs.Controls.Add(new Label { Text = "Name:", Location = new Point(10, 10), AutoSize = true });
            txtName = new TextBox { Location = new Point(10, 30), Width = 150 };
            pnlInputs.Controls.Add(txtName);

            pnlInputs.Controls.Add(new Label { Text = "Category:", Location = new Point(170, 10), AutoSize = true });
            cmbCategory = new ComboBox { Location = new Point(170, 30), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new[] { "Weapon", "Vehicle", "Gadget", "Doomsday Device" });
            pnlInputs.Controls.Add(cmbCategory);

            pnlInputs.Controls.Add(new Label { Text = "Condition %:", Location = new Point(300, 10), AutoSize = true });
            numCondition = new NumericUpDown { Location = new Point(300, 30), Width = 80, Minimum = 0, Maximum = 100, Value = 100 };
            pnlInputs.Controls.Add(numCondition);

            pnlInputs.Controls.Add(new Label { Text = "Price:", Location = new Point(390, 10), AutoSize = true });
            numPrice = new NumericUpDown { Location = new Point(390, 30), Width = 100, Minimum = 0, Maximum = 10000000, DecimalPlaces = 2 };
            pnlInputs.Controls.Add(numPrice);

            pnlInputs.Controls.Add(new Label { Text = "Maint. Cost:", Location = new Point(500, 10), AutoSize = true });
            numMaintenanceCost = new NumericUpDown { Location = new Point(500, 30), Width = 100, Minimum = 0, Maximum = 1000000, DecimalPlaces = 2 };
            pnlInputs.Controls.Add(numMaintenanceCost);

            chkSpecialist = new CheckBox { Text = "Requires Specialist", Location = new Point(620, 30), AutoSize = true };
            pnlInputs.Controls.Add(chkSpecialist);

            // Buttons
            btnAdd = new Button { Text = "Add", Location = new Point(10, 80), Width = 80, Height = 40 };
            btnAdd.Click += BtnAdd_Click;
            pnlInputs.Controls.Add(btnAdd);

            btnUpdate = new Button { Text = "Update", Location = new Point(100, 80), Width = 80, Height = 40 };
            btnUpdate.Click += BtnUpdate_Click;
            pnlInputs.Controls.Add(btnUpdate);

            btnDelete = new Button { Text = "Delete", Location = new Point(190, 80), Width = 80, Height = 40 };
            btnDelete.Click += BtnDelete_Click;
            pnlInputs.Controls.Add(btnDelete);

            btnRepair = new Button { Text = "Repair (Maint.)", Location = new Point(300, 80), Width = 140, Height = 40, BackColor = Color.LightGreen };
            btnRepair.Click += BtnRepair_Click;
            pnlInputs.Controls.Add(btnRepair);
        }

        private void DgvEquipment_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEquipment.SelectedRows.Count > 0)
            {
                int id = (int)dgvEquipment.SelectedRows[0].Cells["EquipmentId"].Value;
                var equipment = _repository.GetEquipmentById(id);
                if (equipment != null)
                {
                    txtName.Text = equipment.Name;
                    cmbCategory.SelectedItem = equipment.Category;
                    numCondition.Value = equipment.Condition;
                    numPrice.Value = equipment.PurchasePrice;
                    numMaintenanceCost.Value = equipment.MaintenanceCost;
                    chkSpecialist.Checked = equipment.RequiresSpecialist;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var equipment = new VillainLairManager.Models.Equipment
                {
                    Name = txtName.Text,
                    Category = cmbCategory.SelectedItem?.ToString(),
                    Condition = (int)numCondition.Value,
                    PurchasePrice = numPrice.Value,
                    MaintenanceCost = numMaintenanceCost.Value,
                    RequiresSpecialist = chkSpecialist.Checked,
                    LastMaintenanceDate = DateTime.Now
                };

                _equipmentService.AddEquipment(equipment);
                LoadData();
                MessageBox.Show("Equipment added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvEquipment.SelectedRows.Count == 0) return;
            
            try
            {
                int id = (int)dgvEquipment.SelectedRows[0].Cells["EquipmentId"].Value;
                // We need to get the original object to preserve other fields like AssignedToSchemeId
                var equipment = _repository.GetEquipmentById(id);
                
                equipment.Name = txtName.Text;
                equipment.Category = cmbCategory.SelectedItem?.ToString();
                equipment.Condition = (int)numCondition.Value;
                equipment.PurchasePrice = numPrice.Value;
                equipment.MaintenanceCost = numMaintenanceCost.Value;
                equipment.RequiresSpecialist = chkSpecialist.Checked;

                _equipmentService.UpdateEquipment(equipment);
                LoadData();
                MessageBox.Show("Equipment updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEquipment.SelectedRows.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to delete this equipment?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    int id = (int)dgvEquipment.SelectedRows[0].Cells["EquipmentId"].Value;
                    _equipmentService.DeleteEquipment(id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRepair_Click(object sender, EventArgs e)
        {
            if (dgvEquipment.SelectedRows.Count == 0) return;

            try
            {
                int id = (int)dgvEquipment.SelectedRows[0].Cells["EquipmentId"].Value;
                // Assuming unlimited funds for manual repair in this form for now
                decimal cost = _equipmentService.PerformMaintenance(id, decimal.MaxValue);
                
                LoadData();
                MessageBox.Show($"Maintenance performed! Cost: {cost:C}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Maintenance failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            var equipmentList = _repository.GetAllEquipment();
            
            // Create a projection for display
            var displayList = equipmentList.Select(e => new 
            {
                e.EquipmentId,
                e.Name,
                e.Category,
                Condition = e.Condition + "%",
                e.PurchasePrice,
                Status = e.AssignedToSchemeId.HasValue ? "Assigned" : (e.StoredAtBaseId.HasValue ? "Stored" : "Unknown"),
                SpecialistRequired = e.RequiresSpecialist ? "Yes" : "No"
            }).ToList();

            dgvEquipment.DataSource = displayList;
        }
    }
}
