Feature: Multiple consumers poll messages from same topic.

Scenario: Multiple consumers not in a consumer group poll messages
	Given Messages are available in topic on single partition
	When Consumers polls messages
	Then Each consumer gets equal amount of messages
	
Scenario: Multiple consumers in a consumer group poll messages
	Given Messages are available in topic on multiple partitions
	When Consumer group polls messages
	Then Each consumer gets messages from server-side calculated partitions

Scenario: Multiple consumers in a consumer group poll messages, one of them disconnects
	Given Messages are available in topic on several partitions 
	When Consumer group polls batch of messages 
	And One consumer disconnects
	Then Consumer group gets rebalanced 
