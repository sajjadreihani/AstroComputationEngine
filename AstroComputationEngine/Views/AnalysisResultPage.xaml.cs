namespace AstroComputationEngine;

public partial class AnalysisResultPage : ContentPage
{
    public string AnalysisText { get; set; }

    public AnalysisResultPage(string analysisText)
    {
        InitializeComponent();
        AnalysisText = analysisText;
        BindingContext = this;
    }

    private async void OnReturnClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}