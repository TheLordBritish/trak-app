using System.Windows.Input;
using SparkyStudios.TrakLibrary.Model.Games;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Controls.Games
{
    public partial class GameUserEntryStatusControl
    {
        public static readonly BindableProperty StatusProperty = 
            BindableProperty.Create(nameof(Status), typeof(GameUserEntryStatus), typeof(GameUserEntryStatusControl));
        
        public static readonly BindableProperty StatusTappedCommandProperty =
            BindableProperty.Create(nameof(StatusTappedCommand), typeof(ICommand), typeof(GameUserEntryStatusControl));
        
        public GameUserEntryStatusControl()
        {
            InitializeComponent();
        }
        
        public GameUserEntryStatus Status
        {
            get => (GameUserEntryStatus) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
        
        public ICommand StatusTappedCommand
        {
            get => (ICommand) GetValue(StatusTappedCommandProperty);
            set => SetValue(StatusTappedCommandProperty, value);
        }
    }
}