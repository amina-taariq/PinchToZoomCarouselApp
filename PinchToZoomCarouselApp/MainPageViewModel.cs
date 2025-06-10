using System.Collections.ObjectModel;
using System.ComponentModel;
namespace PinchToZoomCarouselApp;

public class MainPageViewModel : INotifyPropertyChanged
{
    private ObservableCollection<string> imagesList = new();
    public ObservableCollection<string> ImagesList
    {
        get => imagesList;
        set
        {
            if (imagesList != value)
            {
                imagesList = value;
                OnPropertyChanged(nameof(ImagesList));
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private bool isIndicatorVisible = false;
    public bool IsIndicatorVisible
    {
        get => isIndicatorVisible;
        set
        {
            if (isIndicatorVisible != value)
            {
                isIndicatorVisible = value;
                OnPropertyChanged(nameof(IsIndicatorVisible));
            }
        }
    }
    public MainPageViewModel()
    {
        ImagesList = new ObservableCollection<string>
            {

                "image1.png",
                "image2.png",
                "image3.png",
                "image4.png"
            };
        IsIndicatorVisible = ImagesList.Count > 1;
    }
}