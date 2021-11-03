namespace RabbitMQforNETCore.Models
{
    public enum ExchangeDesc
    {
        QueueDeclare,
        DirectExchange,
        TopicExchange,
        HeaderExchange,
        FanoutExchange
    }
    public enum SendRec
    {
        Publisher,
        Consumer
    }

}
