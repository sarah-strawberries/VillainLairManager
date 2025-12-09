using System;
using System.Windows.Forms;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Minion management form - STUB for students to implement
    /// Should contain CRUD operations with business logic in event handlers
    /// </summary>
    public partial class MinionManagementForm : Form
    {
        private readonly IRepository _repository;
        private readonly IMinionService _minionService;

        public MinionManagementForm(IRepository repository, IMinionService minionService)
        {
            _repository = repository;
            _minionService = minionService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Minion Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Minion Management Form\n\n" +
                       "Requirements:\n" +
                       "- DataGridView showing all minions\n" +
                       "- Text boxes for: Name, Specialty, Skill Level, Salary\n" +
                       "- ComboBox for Base assignment\n" +
                       "- ComboBox for Scheme assignment\n" +
                       "- Buttons: Add, Update, Delete, Refresh\n" +
                       "- All validation logic in button click handlers (anti-pattern)\n" +
                       "- Direct database calls from event handlers (anti-pattern)\n" +
                       "- Loyalty calculation duplicated here (anti-pattern)",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
