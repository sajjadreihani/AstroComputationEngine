using AstroComputationEngine.Interfaces;
using AstroComputationEngine.Models.AI;
using AstroComputationEngine.Models.Chart;
using AstroComputationEngine.Models.City;
using AstroComputationEngine.Utility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstroComputationEngine
{
    public partial class CompositePage : ContentPage
    {
        private readonly IAIService aIService;
        private readonly IChartService chartService;
        private readonly ICityService cityService;

        CityResultDto firstCity = new();

        CityResultDto secondCity = new();

        ObservableCollection<CitySearchResponseData> firstCities = [];
        public ObservableCollection<CitySearchResponseData> FirstCities { get { return firstCities; } }

        ObservableCollection<CitySearchResponseData> secondCities = [];
        public ObservableCollection<CitySearchResponseData> SecondCities { get { return secondCities; } }

        public bool IsValid => firstCity?.Latitude > 0 && secondCity?.Latitude > 0;

        bool IsWholeDay = false;

        public List<AiModel> Models { get; set; }

        private AiModel _selectedModel;
        public AiModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OnPropertyChanged(nameof(SelectedModel));
                }
            }
        }

        public CompositePage(IAIService aIService, IChartService chartService, ICityService cityService)
        {
            this.aIService = aIService;
            this.chartService = chartService;
            this.cityService = cityService;
            Models = AiHelper.Models;

            SelectedModel = Models.First();

            InitializeComponent();
            BindingContext = this;
        }

        private async void OnPromptClicked(object? sender, EventArgs e)
        {
            try
            {
                var prompt = new StringBuilder();
                prompt.AppendLine(GenerateChart());
                prompt.AppendLine();
                prompt.AppendLine("Analyze Chart");
                
                await Clipboard.Default.SetTextAsync(prompt.ToString());

            }
            catch (Exception ex)
            {
                AnalyzeResult.Text = ex.Message;
            }

        }

        private async void OnCopyClicked(object? sender, EventArgs e)
        {
            try
            {
                await Clipboard.Default.SetTextAsync(GenerateChart());

            }
            catch (Exception ex)
            {
                AnalyzeResult.Text = ex.Message;
            }
        }

        private async void OnAIClicked(object? sender, EventArgs e)
        {
            try
            {
                AnalyzeResult.Text = "Analyzing...";

                var response = await aIService.AskAI(GenerateChart(), SelectedModel.Id);

                await Navigation.PushAsync(new AnalysisResultPage(response.Choices.FirstOrDefault()?.Message.Content));

                AnalyzeResult.Text = "";
            }
            catch (Exception ex)
            {
                AnalyzeResult.Text = ex.Message;
            }
        }

        private string GenerateChart()
        {
            var firstDateTime = FirstDatePicker.Date + FirstTimePicker.Time;
            var secondDateTime = SecondDatePicker.Date + SecondTimePicker.Time;

            return chartService.GenerateComposite(new(new TimeLocation(firstDateTime.Value, firstCity.Latitude, firstCity.Longitude, firstCity.Name, firstCity.TimeZoneName)
                , new TimeLocation(secondDateTime.Value, secondCity.Latitude, secondCity.Longitude, secondCity.Name, secondCity.TimeZoneName), CurrentDatePicker.Date.Value));

        }

        private async void FirstCitySearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 2)
            {
                FirstCityResults.ItemsSource = null;
                return;
            }

            try
            {
                firstCities = [..(await cityService.Search(e.NewTextValue)).ToList()];
                FirstCityResults.ItemsSource = firstCities;
                if(firstCities.Any())
                    FirstCityResults.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to search cities: {ex.Message}", "OK");
            }
        }

        private void FirstCityResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection.FirstOrDefault() is CitySearchResponseData selectedCity)
                {
                    firstCity = new CityResultDto()
                    {
                        Latitude = selectedCity.Latitude,
                        Longitude = selectedCity.Longitude,
                        Name = selectedCity.DisplayName,
                        TimeZoneName = selectedCity.Timezone
                    };
                    FirstCitySearchBar.Text = selectedCity.DisplayName;
                    FirstCityResults.ItemsSource = null;
                    FirstCityResults.IsVisible = false;

                    OnPropertyChanged(nameof(IsValid));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void SecondCitySearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 2)
            {
                SecondCityResults.ItemsSource = null;
                return;
            }

            try
            {
                secondCities = [.. (await cityService.Search(e.NewTextValue)).ToList()];
                SecondCityResults.ItemsSource = secondCities;
                if (secondCities.Any())
                    SecondCityResults.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to search cities: {ex.Message}", "OK");
            }
        }

        private async void CurrentCityResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CitySearchResponseData selectedCity)
            {
                secondCity = new CityResultDto()
                {
                    Latitude = selectedCity.Latitude,
                    Longitude = selectedCity.Longitude,
                    Name = selectedCity.DisplayName,
                    TimeZoneName = selectedCity.Timezone
                };
                SecondCitySearchBar.Text = selectedCity.DisplayName;
                SecondCityResults.ItemsSource = null;
                SecondCityResults.IsVisible = false;

                OnPropertyChanged(nameof(IsValid));
            }
        }
    }
}
