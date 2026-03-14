using Microsoft.ML.Data;

namespace Echo.Infrastructure.ML;

public class MessageSentimentData
{
    [ColumnName("Content"), LoadColumn(0)]
    public string Content { get; set; }

    [ColumnName("Label"), LoadColumn(1)]
    public bool IsToxic { get; set; }
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }
}