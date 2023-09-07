using FluentAssertions;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.MessageStream;
using Iggy_SDK_Tests.Utils.Messages;
using TechTalk.SpecFlow;

namespace Iggy_SDK_Tests.BehaviorTests.Steps;

[Binding]
public sealed class MultipleConsumersPollingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IMessageStream _messageStream;
    private readonly int[] _listOfIds;

    private readonly int[] _consumersIds = new int[] {1,2,3};

    public MultipleConsumersPollingSteps(ScenarioContext scenarioContext, IMessageStream messageStream, int[] listOfIds)
    {
        _scenarioContext = scenarioContext;
        _messageStream = messageStream;
        _listOfIds = listOfIds;

    }

    [When(@"Consumers poll messages")]
    public async Task WhenConsumersPollMessages()
    {
        var consumerPolledMessages = new List<PolledMessages>();
        foreach(var consumerId in _consumersIds)
        {
            var request = MessageFactory.CreateMessageFetchRequest(10, _listOfIds[0], _listOfIds[1], 1 , consumerId);
            var result = await _messageStream.PollMessagesAsync(request);
            consumerPolledMessages.Add(result);
        }
        _scenarioContext.Add("ConsumersPollResults", consumerPolledMessages);
    }

    [Then(@"Each consumer gets equal amount of messages")]
    public void ThenEachConsumerGetsEqualAmountOfMessages()
    {
        var consumerPolledMessages = _scenarioContext.Get<List<PolledMessages>>("ConsumersPollResults");
        
        foreach(var polledMessage in consumerPolledMessages)
        {
            var xd = polledMessage.Messages.Count().Should().Be(10);
            break;
        }

    }

    [When(@"Consumer group poll messages")]
    public void WhenConsumerGroupPollMessages()
    {
        ScenarioContext.StepIsPending();
    }

    [Then(@"Each consumer gets same amount of messages")]
    public void ThenEachConsumerGetsSameAmountOfMessages()
    {
        ScenarioContext.StepIsPending();
    }
}