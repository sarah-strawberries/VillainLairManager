using System;
using System.Windows.Forms;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class EquipmentInventoryForm : Form
    {
        private readonly IRepository _repository;
        private readonly IEquipmentService _equipmentService;

        public EquipmentInventoryForm(IRepository repository, IEquipmentService equipmentService)
        {
            _repository = repository;
            _equipmentService = equipmentService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Equipment Inventory";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Equipment Inventory Form",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
