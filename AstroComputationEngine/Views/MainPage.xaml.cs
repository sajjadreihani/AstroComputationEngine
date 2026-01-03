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
    public partial class MainPage : ContentPage
    {
        private readonly IAIService aIService;
        private readonly IChartService chartService;
        private readonly ICityService cityService;

        CityResultDto birthCity = new();

        CityResultDto currentCity = new();

        ObservableCollection<CitySearchResponseData> birthCities = [];
        public ObservableCollection<CitySearchResponseData> BirthCities { get { return birthCities; } }

        ObservableCollection<CitySearchResponseData> currentCities = [];
        public ObservableCollection<CitySearchResponseData> CurrentCities { get { return currentCities; } }

        public bool IsValid => birthCity?.Latitude > 0 && currentCity?.Latitude > 0;

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

        public MainPage(IAIService aIService, IChartService chartService, ICityService cityService)
        {
            this.aIService = aIService;
            this.chartService = chartService;
            this.cityService = cityService;            
            Models = AiHelper.Models;

            SelectedModel = Models.First();

            InitializeComponent();
            BindingContext = this;
        }

        void OnWholeDayClicked(object sender, CheckedChangedEventArgs e)
        {
            IsWholeDay = e.Value;

            CurrentTimePicker.IsVisible = !IsWholeDay;
        }

        private async void OnPromptClicked(object? sender, EventArgs e)
        {
            try
            {
                var prompt = new StringBuilder();
                prompt.AppendLine(GenerateChart());
                prompt.AppendLine();
                prompt.AppendLine("Analyze the chart");
                
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
            var birthDateTime = BirthDatePicker.Date + BirthTimePicker.Time;
            var currentDateTime = CurrentDatePicker.Date + CurrentTimePicker.Time;

            return IsWholeDay ? chartService.GenerateDaily(new DailyChartInput(new TimeLocation(birthDateTime.Value, birthCity.Latitude, birthCity.Longitude, birthCity.Name, birthCity.TimeZoneName), new TimeLocation(currentDateTime.Value, currentCity.Latitude, currentCity.Longitude, currentCity.Name, currentCity.TimeZoneName)))
                : chartService.GenerateMoment(new DailyChartInput(new TimeLocation(birthDateTime.Value, birthCity.Latitude, birthCity.Longitude, birthCity.Name, birthCity.TimeZoneName), new TimeLocation(currentDateTime.Value, currentCity.Latitude, currentCity.Longitude, currentCity.Name, currentCity.TimeZoneName)));

        }

        private async void BirthCitySearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 2)
            {
                BirthCityResults.ItemsSource = null;
                return;
            }

            try
            {
                birthCities = [..(await cityService.Search(e.NewTextValue)).ToList()];
                BirthCityResults.ItemsSource = birthCities;
                if(birthCities.Any())
                    BirthCityResults.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to search cities: {ex.Message}", "OK");
            }
        }

        private void BirthCityResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection.FirstOrDefault() is CitySearchResponseData selectedCity)
                {
                    birthCity = new CityResultDto()
                    {
                        Latitude = selectedCity.Latitude,
                        Longitude = selectedCity.Longitude,
                        Name = selectedCity.DisplayName,
                        TimeZoneName = selectedCity.Timezone
                    };
                    BirthCitySearchBar.Text = selectedCity.DisplayName;
                    BirthCityResults.ItemsSource = null;
                    BirthCityResults.IsVisible = false;

                    OnPropertyChanged(nameof(IsValid));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void CurrentCitySearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 2)
            {
                CurrentCityResults.ItemsSource = null;
                return;
            }

            try
            {
                currentCities = [.. (await cityService.Search(e.NewTextValue)).ToList()];
                CurrentCityResults.ItemsSource = currentCities;
                if (currentCities.Any())
                    CurrentCityResults.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to search cities: {ex.Message}", "OK");
            }
        }

        private void CurrentCityResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CitySearchResponseData selectedCity)
            {
                currentCity = new CityResultDto()
                {
                    Latitude = selectedCity.Latitude,
                    Longitude = selectedCity.Longitude,
                    Name = selectedCity.DisplayName,
                    TimeZoneName = selectedCity.Timezone
                };

                CurrentCitySearchBar.Text = selectedCity.DisplayName;
                CurrentCityResults.ItemsSource = null;
                CurrentCityResults.IsVisible = false;

                OnPropertyChanged(nameof(IsValid));
            }
        }
    }
}
