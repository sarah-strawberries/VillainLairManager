using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class BaseManagementForm : Form
    {
        private readonly IRepository _repository;
        private readonly ISecretBaseService _baseService;

        // UI Controls
        private DataGridView _baseGridView;
        private Label _lblSelectedBase;
        private Label _lblLocation;
        private Label _lblSecurityLevel;
        private Label _lblOccupancy;
        private ProgressBar _occupancyProgressBar;
        private Label _lblDoomsdayStatus;
        private Label _lblDiscoveryStatus;
        private Label _lblMaintenanceCost;
        private ListBox _lstMinions;
        private ListBox _lstEquipment;
        private Button _btnNew;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnAssignResources;
        private Button _btnRepair;

        private SecretBase _selectedBase;

        public BaseManagementForm(IRepository repository, ISecretBaseService baseService)
        {
            _repository = repository;
            _baseService = baseService;
            InitializeComponent();
            
            // Initialize the service's bases cache
            ((SecretBaseService)_baseService).InitializeBases();
            
            LoadBases();
        }

        private void InitializeComponent()
        {
            this.Text = "Secret Base Management";
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

            // LEFT PANEL: Base List Grid
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            splitContainer.Panel1.Controls.Add(pnlLeft);

            var lblBaseList = new Label
            {
                Text = "Secret Bases",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlLeft.Controls.Add(lblBaseList);

            _baseGridView = new DataGridView
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
            _baseGridView.Columns.Add("BaseId", "ID");
            _baseGridView.Columns.Add("Name", "Base Name");
            _baseGridView.Columns.Add("Location", "Location");
            _baseGridView.Columns.Add("Capacity", "Capacity");
            _baseGridView.Columns.Add("Security", "Security");
            _baseGridView.Columns.Add("Occupancy", "Occupancy");
            _baseGridView.Columns.Add("Discovery", "Discovery");
            _baseGridView.CellClick += BaseGridView_CellClick;
            pnlLeft.Controls.Add(_baseGridView);

            // Button toolbar
            var pnlButtonToolbar = new Panel
            {
                Location = new Point(0, 540),
                Size = new Size(480, 50),
                BackColor = Color.White
            };
            pnlLeft.Controls.Add(pnlButtonToolbar);

            _btnNew = new Button
            {
                Text = "New",
                Location = new Point(0, 0),
                Size = new Size(60, 40),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _btnNew.Click += BtnNew_Click;
            pnlButtonToolbar.Controls.Add(_btnNew);

            _btnEdit = new Button
            {
                Text = "Edit",
                Location = new Point(65, 0),
                Size = new Size(60, 40),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _btnEdit.Click += BtnEdit_Click;
            pnlButtonToolbar.Controls.Add(_btnEdit);

            _btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(130, 0),
                Size = new Size(60, 40),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _btnDelete.Click += BtnDelete_Click;
            pnlButtonToolbar.Controls.Add(_btnDelete);

            _btnAssignResources = new Button
            {
                Text = "Assign",
                Location = new Point(195, 0),
                Size = new Size(60, 40),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _btnAssignResources.Click += BtnAssignResources_Click;
            pnlButtonToolbar.Controls.Add(_btnAssignResources);

            _btnRepair = new Button
            {
                Text = "Repair",
                Location = new Point(260, 0),
                Size = new Size(60, 40),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _btnRepair.Click += BtnRepair_Click;
            pnlButtonToolbar.Controls.Add(_btnRepair);

            // RIGHT PANEL: Base Details
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                AutoScroll = true
            };
            splitContainer.Panel2.Controls.Add(pnlRight);

            var lblDetailsTitle = new Label
            {
                Text = "Base Details",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlRight.Controls.Add(lblDetailsTitle);

            _lblSelectedBase = new Label
            {
                Text = "No base selected",
                Font = new Font("Arial", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 30),
                ForeColor = Color.DarkBlue
            };
            pnlRight.Controls.Add(_lblSelectedBase);

            _lblLocation = new Label
            {
                Text = "Location: -",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(0, 60)
            };
            pnlRight.Controls.Add(_lblLocation);

            var lblSecurityLabel = new Label
            {
                Text = "Security Level:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 85)
            };
            pnlRight.Controls.Add(lblSecurityLabel);

            _lblSecurityLevel = new Label
            {
                Text = "-",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(120, 85)
            };
            pnlRight.Controls.Add(_lblSecurityLevel);

            var lblOccupancyLabel = new Label
            {
                Text = "Occupancy:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 110)
            };
            pnlRight.Controls.Add(lblOccupancyLabel);

            _occupancyProgressBar = new ProgressBar
            {
                Location = new Point(0, 135),
                Size = new Size(200, 25),
                Style = ProgressBarStyle.Continuous
            };
            pnlRight.Controls.Add(_occupancyProgressBar);

            _lblOccupancy = new Label
            {
                Text = "0 / 0",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(210, 140)
            };
            pnlRight.Controls.Add(_lblOccupancy);

            var lblDoomsdayLabel = new Label
            {
                Text = "Doomsday Device:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 170)
            };
            pnlRight.Controls.Add(lblDoomsdayLabel);

            _lblDoomsdayStatus = new Label
            {
                Text = "No",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(160, 170),
                ForeColor = Color.Red
            };
            pnlRight.Controls.Add(_lblDoomsdayStatus);

            var lblDiscoveryLabel = new Label
            {
                Text = "Discovery Status:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 195)
            };
            pnlRight.Controls.Add(lblDiscoveryLabel);

            _lblDiscoveryStatus = new Label
            {
                Text = "Safe",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(160, 195),
                ForeColor = Color.Green
            };
            pnlRight.Controls.Add(_lblDiscoveryStatus);

            var lblMaintenanceLabel = new Label
            {
                Text = "Monthly Maintenance:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 220)
            };
            pnlRight.Controls.Add(lblMaintenanceLabel);

            _lblMaintenanceCost = new Label
            {
                Text = "0",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(160, 220)
            };
            pnlRight.Controls.Add(_lblMaintenanceCost);

            var lblMinionsLabel = new Label
            {
                Text = "Stationed Minions:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 250)
            };
            pnlRight.Controls.Add(lblMinionsLabel);

            _lstMinions = new ListBox
            {
                Location = new Point(0, 275),
                Size = new Size(300, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            pnlRight.Controls.Add(_lstMinions);

            var lblEquipmentLabel = new Label
            {
                Text = "Stored Equipment:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 435)
            };
            pnlRight.Controls.Add(lblEquipmentLabel);

            _lstEquipment = new ListBox
            {
                Location = new Point(0, 460),
                Size = new Size(300, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            pnlRight.Controls.Add(_lstEquipment);
        }

        private void LoadBases()
        {
            _baseGridView.Rows.Clear();
            var bases = _repository.GetAllBases();

            foreach (var base_ in bases)
            {
                int occupancy = GetOccupancy(base_.BaseId);
                string discoveryStatus = base_.IsDiscovered ? "Discovered" : "Safe";

                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell { Value = base_.BaseId });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = base_.Name });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = base_.Location });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = base_.Capacity });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = base_.SecurityLevel });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = $"{occupancy}/{base_.Capacity}" });

                var discoveryCell = new DataGridViewTextBoxCell { Value = discoveryStatus };
                if (base_.IsDiscovered)
                {
                    discoveryCell.Style.BackColor = Color.LightCoral;
                    discoveryCell.Style.ForeColor = Color.DarkRed;
                }
                else
                {
                    discoveryCell.Style.BackColor = Color.LightGreen;
                    discoveryCell.Style.ForeColor = Color.DarkGreen;
                }
                row.Cells.Add(discoveryCell);

                _baseGridView.Rows.Add(row);
            }
        }

        private int GetOccupancy(int baseId)
        {
            var minions = _repository.GetAllMinions();
            return minions.Count(m => m.CurrentBaseId == baseId);
        }

        private void BaseGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int baseId = int.Parse(_baseGridView.Rows[e.RowIndex].Cells[0].Value.ToString());
            _selectedBase = _repository.GetAllBases().FirstOrDefault(b => b.BaseId == baseId);

            if (_selectedBase != null)
            {
                UpdateDetailsPanel();
            }
        }

        private void UpdateDetailsPanel()
        {
            if (_selectedBase == null)
            {
                _lblSelectedBase.Text = "No base selected";
                return;
            }

            _lblSelectedBase.Text = _selectedBase.Name;
            _lblLocation.Text = $"Location: {_selectedBase.Location}";
            _lblSecurityLevel.Text = _selectedBase.SecurityLevel.ToString();
            _lblMaintenanceCost.Text = $"${_selectedBase.MonthlyMaintenanceCost:N0}";

            // Occupancy
            int occupancy = GetOccupancy(_selectedBase.BaseId);
            _lblOccupancy.Text = $"{occupancy} / {_selectedBase.Capacity}";
            _occupancyProgressBar.Maximum = _selectedBase.Capacity;
            _occupancyProgressBar.Value = Math.Min(occupancy, _selectedBase.Capacity);

            // Color occupancy bar
            if (occupancy >= _selectedBase.Capacity)
            {
                _occupancyProgressBar.ForeColor = Color.Red;
            }
            else if (occupancy >= _selectedBase.Capacity * 0.75)
            {
                _occupancyProgressBar.ForeColor = Color.Orange;
            }
            else
            {
                _occupancyProgressBar.ForeColor = Color.Green;
            }

            // Doomsday Device
            if (_selectedBase.HasDoomsdayDevice)
            {
                _lblDoomsdayStatus.Text = "Yes";
                _lblDoomsdayStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                _lblDoomsdayStatus.Text = "No";
                _lblDoomsdayStatus.ForeColor = Color.Red;
            }

            // Discovery Status
            if (_selectedBase.IsDiscovered)
            {
                _lblDiscoveryStatus.Text = "Discovered";
                _lblDiscoveryStatus.ForeColor = Color.DarkRed;
            }
            else
            {
                _lblDiscoveryStatus.Text = "Safe";
                _lblDiscoveryStatus.ForeColor = Color.DarkGreen;
            }

            // Minions at base
            var minions = _repository.GetAllMinions().Where(m => m.CurrentBaseId == _selectedBase.BaseId).ToList();
            _lstMinions.Items.Clear();
            foreach (var minion in minions)
            {
                _lstMinions.Items.Add($"{minion.Name} ({minion.Specialty})");
            }

            // Equipment stored at base
            var equipment = _repository.GetAllEquipment().Where(e => e.StoredAtBaseId == _selectedBase.BaseId).ToList();
            _lstEquipment.Items.Clear();
            foreach (var item in equipment)
            {
                _lstEquipment.Items.Add($"{item.Name} ({item.Condition}%)");
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            var dialog = new BaseEditDialog(_repository, _baseService, null);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ((SecretBaseService)_baseService).InitializeBases();
                LoadBases();
                UpdateDetailsPanel();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedBase == null)
            {
                MessageBox.Show("Please select a base to edit", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dialog = new BaseEditDialog(_repository, _baseService, _selectedBase);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ((SecretBaseService)_baseService).InitializeBases();
                LoadBases();
                UpdateDetailsPanel();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedBase == null)
            {
                MessageBox.Show("Please select a base to delete", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedBase.Name}'?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _repository.DeleteBase(_selectedBase.BaseId);
                _selectedBase = null;
                LoadBases();
                UpdateDetailsPanel();
            }
        }

        private void BtnAssignResources_Click(object sender, EventArgs e)
        {
            if (_selectedBase == null)
            {
                MessageBox.Show("Please select a base first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create context menu for resource assignment
            var contextMenu = new ContextMenuStrip();
            var assignMinionsItem = new ToolStripMenuItem("Assign Minions");
            var storeEquipmentItem = new ToolStripMenuItem("Store Equipment");

            assignMinionsItem.Click += (s, e2) => ShowAssignMinionsDialog();
            storeEquipmentItem.Click += (s, e2) => ShowStoreEquipmentDialog();

            contextMenu.Items.Add(assignMinionsItem);
            contextMenu.Items.Add(storeEquipmentItem);

            contextMenu.Show(MousePosition);
        }

        private void ShowAssignMinionsDialog()
        {
            var dialog = new AssignMinionsToBaseDialog(_repository, _selectedBase);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ((SecretBaseService)_baseService).InitializeBases();
                LoadBases();
                UpdateDetailsPanel();
            }
        }

        private void ShowStoreEquipmentDialog()
        {
            var dialog = new StoreEquipmentDialog(_repository, _selectedBase);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ((SecretBaseService)_baseService).InitializeBases();
                LoadBases();
                UpdateDetailsPanel();
            }
        }

        private void BtnRepair_Click(object sender, EventArgs e)
        {
            if (_selectedBase == null)
            {
                MessageBox.Show("Please select a base first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show("Repair functionality coming soon", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
