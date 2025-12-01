using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Main dashboard form with navigation and statistics
    /// Contains business logic in UI layer (anti-pattern)
    /// </summary>
    public partial class MainForm : Form
    {
        private Button btnMinions;
        private Button btnSchemes;
        private Button btnBases;
        private Button btnEquipment;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblMinionStats;
        private Label lblSchemeStats;
        private Label lblCostStats;
        private Panel pnlAlerts;
        private Label lblAlerts;

        public MainForm()
        {
            InitializeComponent();
            LoadStatistics(); // Business logic in form load (anti-pattern)
        }

        private void InitializeComponent()
        {
            this.Text = "Super Villain Lair Management System";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title
            lblTitle = new Label
            {
                Text = "🦹 Super Villain Lair Management System 🦹",
                Font = new Font("Arial", 20, FontStyle.Bold),
                Location = new Point(150, 20),
                Size = new Size(500, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Navigation buttons
            btnMinions = CreateNavigationButton("Manage Minions", 100, 100);
            btnSchemes = CreateNavigationButton("Manage Evil Schemes", 450, 100);
            btnBases = CreateNavigationButton("Manage Secret Bases", 100, 200);
            btnEquipment = CreateNavigationButton("Equipment Inventory", 450, 200);

            btnMinions.Click += (s, e) => OpenForm(new MinionManagementForm());
            btnSchemes.Click += (s, e) => OpenForm(new SchemeManagementForm());
            btnBases.Click += (s, e) => OpenForm(new BaseManagementForm());
            btnEquipment.Click += (s, e) => OpenForm(new EquipmentInventoryForm());

            // Statistics panel
            pnlStats = new Panel
            {
                Location = new Point(50, 320),
                Size = new Size(700, 150),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblMinionStats = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(680, 30),
                Font = new Font("Arial", 10)
            };
            pnlStats.Controls.Add(lblMinionStats);

            lblSchemeStats = new Label
            {
                Location = new Point(10, 50),
                Size = new Size(680, 30),
                Font = new Font("Arial", 10)
            };
            pnlStats.Controls.Add(lblSchemeStats);

            lblCostStats = new Label
            {
                Location = new Point(10, 90),
                Size = new Size(680, 30),
                Font = new Font("Arial", 10)
            };
            pnlStats.Controls.Add(lblCostStats);

            this.Controls.Add(pnlStats);

            // Alerts panel
            pnlAlerts = new Panel
            {
                Location = new Point(50, 480),
                Size = new Size(700, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            lblAlerts = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(680, 60),
                Font = new Font("Arial", 9),
                ForeColor = Color.DarkRed
            };
            pnlAlerts.Controls.Add(lblAlerts);

            this.Controls.Add(pnlAlerts);
        }

        private Button CreateNavigationButton(string text, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(250, 80),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(btn);
            return btn;
        }

        private void OpenForm(Form form)
        {
            form.ShowDialog();
            LoadStatistics(); // Refresh after closing child form
        }

        // Business logic in UI layer (anti-pattern)
        // This calculation is duplicated from models
        private void LoadStatistics()
        {
            // Direct database access from UI (anti-pattern)
            var minions = DatabaseHelper.GetAllMinions();
            var schemes = DatabaseHelper.GetAllSchemes();
            var bases = DatabaseHelper.GetAllBases();
            var equipment = DatabaseHelper.GetAllEquipment();

            // Minion statistics with duplicated mood calculation
            int happyCount = 0, grumpyCount = 0, betrayalCount = 0;
            foreach (var minion in minions)
            {
                // Mood calculation duplicated from Minion.UpdateMood() (anti-pattern)
                if (minion.LoyaltyScore > 70)
                    happyCount++;
                else if (minion.LoyaltyScore < 40)
                    betrayalCount++;
                else
                    grumpyCount++;
            }

            lblMinionStats.Text = $"Minions: {minions.Count} total | Happy: {happyCount} | Grumpy: {grumpyCount} | Plotting Betrayal: {betrayalCount}";

            // Scheme statistics with duplicated success calculation
            var activeSchemes = schemes.Where(s => s.Status == "Active").ToList();
            double avgSuccess = 0;
            if (activeSchemes.Any())
            {
                // Success likelihood calculation duplicated here (anti-pattern)
                foreach (var scheme in activeSchemes)
                {
                    // This is also in EvilScheme.CalculateSuccessLikelihood() - duplication!
                    int success = scheme.CalculateSuccessLikelihood();
                    avgSuccess += success;
                }
                avgSuccess /= activeSchemes.Count;
            }

            lblSchemeStats.Text = $"Evil Schemes: {schemes.Count} total | Active: {activeSchemes.Count} | Avg Success Likelihood: {avgSuccess:F1}%";

            // Cost calculation (business logic in UI)
            decimal totalMinionSalaries = 0;
            foreach (var minion in minions)
            {
                totalMinionSalaries += minion.SalaryDemand;
            }

            decimal totalBaseCosts = 0;
            foreach (var baseObj in bases)
            {
                totalBaseCosts += baseObj.MonthlyMaintenanceCost;
            }

            decimal totalEquipmentCosts = 0;
            foreach (var equip in equipment)
            {
                totalEquipmentCosts += equip.MaintenanceCost;
            }

            decimal totalMonthlyCost = totalMinionSalaries + totalBaseCosts + totalEquipmentCosts;

            lblCostStats.Text = $"Monthly Costs: Minions: ${totalMinionSalaries:N0} | Bases: ${totalBaseCosts:N0} | Equipment: ${totalEquipmentCosts:N0} | TOTAL: ${totalMonthlyCost:N0}";

            // Alerts (more business logic in UI)
            var alerts = "";

            // Low loyalty alert
            var lowLoyaltyMinions = minions.Where(m => m.LoyaltyScore < 40).Count();
            if (lowLoyaltyMinions > 0)
            {
                alerts += $"⚠ Warning: {lowLoyaltyMinions} minions have low loyalty and may betray you! ";
            }

            // Broken equipment alert
            var brokenEquipment = equipment.Where(e => e.Condition < 20).Count();
            if (brokenEquipment > 0)
            {
                alerts += $"⚠ {brokenEquipment} equipment items are broken! ";
            }

            // Over budget schemes
            var overBudgetSchemes = schemes.Where(s => s.CurrentSpending > s.Budget).Count();
            if (overBudgetSchemes > 0)
            {
                alerts += $"⚠ {overBudgetSchemes} schemes are over budget! ";
            }

            lblAlerts.Text = string.IsNullOrEmpty(alerts) ? "✓ All systems operational" : alerts;
        }
    }
}
