using Echo.Application.Interfaces;
using Microsoft.ML;

namespace Echo.Infrastructure.ML;

public class ContentModerationService : IContentModerationService
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public ContentModerationService()
    {
        _mlContext = new MLContext();

        // Тестовый датасет
        var data = new List<MessageSentimentData>
        {
            new() { Content = "терроризм взрыв наркотики", IsToxic = true },
            new() { Content = "купить оружие смерть", IsToxic = true },
            new() { Content = "привет как дела", IsToxic = false },
            new() { Content = "отличный мессенджер", IsToxic = false }
        };

        var trainData = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(MessageSentimentData.Content))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

        _model = pipeline.Fit(trainData);
    }

    public bool IsToxic(string text)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<MessageSentimentData, SentimentPrediction>(_model);
        var result = predictionEngine.Predict(new MessageSentimentData { Content = text });

        return result.Prediction;
    }
}