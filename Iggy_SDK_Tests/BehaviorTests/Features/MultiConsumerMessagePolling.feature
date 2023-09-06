Feature: Multiple consumers poll messages from same topic.

Scenario: Multiple consumers not in a consumer group poll messages
	When Consumers poll messages
	Then Each consumer gets equal amount of messages
	
Scenario: Multiple consumers in a consumer group poll messages
	When Consumer group poll messages
	Then Each consumer gets same amount of messages
