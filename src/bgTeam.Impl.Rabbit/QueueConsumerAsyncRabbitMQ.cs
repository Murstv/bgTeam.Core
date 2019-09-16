﻿namespace bgTeam.Impl.Rabbit
{
    using bgTeam.Queues;
    using bgTeam.Queues.Exceptions;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Threading.Tasks;

    public class QueueConsumerAsyncRabbitMQ : BaseQueueConsumerRabbitMQ<AsyncEventingBasicConsumer>, IQueueWatcher<IQueueMessage>
    {
        public QueueConsumerAsyncRabbitMQ(IConnectionFactory connectionFactory, IMessageProvider provider)
            : this(connectionFactory, provider, PREFETCHCOUNT)
        {
        }

        public QueueConsumerAsyncRabbitMQ(
            IConnectionFactory connectionFactory,
            IMessageProvider provider,
            ushort prefetchCount)
            : base(connectionFactory, provider, prefetchCount)
        {
        }

        private async Task ReceiverHandler(object sender, BasicDeliverEventArgs e)
        {
            var message = _provider.ExtractObject(e.Body);
            try
            {
                await OnSubscribe(message);
            }
            catch (Exception ex)
            {
                OnError(this, new ExtThreadExceptionEventArgs(message, ex));
            }
            finally
            {
                var model = ((EventingBasicConsumer)sender).Model;
                model.BasicAck(e.DeliveryTag, false);
            }
        }

        private async Task ShutdownHandler(object sender, ShutdownEventArgs e)
        {
            InitConsumer();
            StartWatch(_watchingQueueName);
        }

        protected override void InitConsumer()
        {
            if (_consumer != null)
            {
                _consumer.Received -= ReceiverHandler;
                _consumer.Shutdown -= ShutdownHandler;
            }

            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += ReceiverHandler;
            _consumer.Shutdown += ShutdownHandler;
        }
    }
}