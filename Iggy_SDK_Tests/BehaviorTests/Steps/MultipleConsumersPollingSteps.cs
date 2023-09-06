using TechTalk.SpecFlow;

namespace Iggy_SDK_Tests.BehaviorTests.Steps;

[Binding]
public sealed class MultipleConsumersPollingSteps
{
    private readonly ScenarioContext _scenarioContext;

    public MultipleConsumersPollingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"Consumers poll messages")]
    public void WhenConsumersPollMessages()
    {
        ScenarioContext.StepIsPending();
    }

    [Then(@"Each consumer gets equal amount of messages")]
    public void ThenEachConsumerGetsEqualAmountOfMessages()
    {
        ScenarioContext.StepIsPending();
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