namespace Iggy_SDK.Errors;

internal static class ErrorFactory
{
    internal static ErrorModel Error => new ErrorModel(0, "error", "Error");
    internal static ErrorModel IoError()
        => new ErrorModel(1, "io_error", $"IO error");
    internal static ErrorModel InvalidCommand => new ErrorModel(2, "invalid_command", "Invalid command");
    internal static ErrorModel InvalidFormat => new ErrorModel(3, "invalid_format", "Invalid format");
    internal static ErrorModel CannotCreateBaseDirectory => new ErrorModel(4, "cannot_create_base_directory", "Cannot create base directory");
    internal static ErrorModel CannotCreateStreamsDirectory => new ErrorModel(5, "cannot_create_streams_directory", "Cannot create streams directory");
    internal static ErrorModel CannotCreateStreamDirectory(int streamId)
        => new ErrorModel(6, "cannot_create_stream_directory", $"Cannot create stream with ID: {streamId} directory");
    internal static ErrorModel CannotCreateStreamInfo(int streamId)
        => new ErrorModel(7, "cannot_create_stream_info", $"Failed to create stream info file for stream with ID: {streamId}");
    internal static ErrorModel CannotUpdateStreamInfo(int streamId)
        => new ErrorModel(8, "cannot_update_stream_info", $"Failed to update stream info for stream with ID: {streamId}");
    internal static ErrorModel CannotOpenStreamInfo(int streamId)
        => new ErrorModel(9, "cannot_open_stream_info", $"Failed to open stream info file for stream with ID: {streamId}");
    internal static ErrorModel CannotReadStreamInfo(int streamId)
        => new ErrorModel(10, "cannot_read_stream_info", $"Failed to read stream info file for stream with ID: {streamId}");
    internal static ErrorModel CannotCreateStream(int streamId)
        => new ErrorModel(11, "cannot_create_stream", $"Failed to create stream with ID: {streamId}");
    internal static ErrorModel CannotDeleteStream(int streamId)
        => new ErrorModel(12, "cannot_delete_stream", $"Failed to delete stream with ID: {streamId}");
    internal static ErrorModel CannotDeleteStreamDirectory(int streamId)
        => new ErrorModel(13, "cannot_delete_stream_directory", $"Failed to delete stream directory with ID: {streamId}");
    internal static ErrorModel StreamNotFound(int streamId)
        => new ErrorModel(14, "stream_not_found", $"Stream with ID: {streamId} was not found.");
    internal static ErrorModel StreamAlreadyExists(int streamId)
        => new ErrorModel(15, "stream_already_exists", $"Stream with ID: {streamId} already exists.");
    internal static ErrorModel InvalidStreamName => new ErrorModel(16, "invalid_stream_name", "Invalid stream name");
    internal static ErrorModel CannotCreateTopicsDirectory(int streamId)
        => new ErrorModel(17, "cannot_create_topics_directory", $"Cannot create topics directory for stream with ID: {streamId}");
    internal static ErrorModel CannotCreateTopicDirectory(int streamId, int topicId)
        => new ErrorModel(18, "cannot_create_topic_directory", $"Failed to create directory for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreateTopicInfo(int streamId, int topicId)
        => new ErrorModel(19, "cannot_create_topic_info", $"Failed to create topic info file for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotUpdateTopicInfo(int streamId, int topicId)
        => new ErrorModel(20, "cannot_update_topic_info", $"Failed to update topic info for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotOpenTopicInfo(int streamId, int topicId)
        => new ErrorModel(21, "cannot_open_topic_info", $"Failed to open topic info file for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotReadTopicInfo(int streamId, int topicId)
        => new ErrorModel(22, "cannot_read_topic_info", $"Failed to read topic info file for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreateTopic(int streamId, int topicId)
        => new ErrorModel(23, "cannot_create_topic", $"Failed to create topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotDeleteTopic(int streamId, int topicId)
        => new ErrorModel(24, "cannot_delete_topic", $"Failed to delete topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotDeleteTopicDirectory(int streamId, int topicId)
        => new ErrorModel(25, "cannot_delete_topic_directory", $"Failed to delete topic directory with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotPollTopic => new ErrorModel(26, "cannot_poll_topic", "Cannot poll topic");
    internal static ErrorModel TopicNotFound(int streamId, int topicId)
        => new ErrorModel(27, "topic_not_found", $"Topic with ID: {topicId} for stream with ID: {streamId} was not found.");
    internal static ErrorModel TopicAlreadyExists(int streamId, int topicId)
        => new ErrorModel(28, "topic_already_exists", $"Topic with ID: {topicId} for stream with ID: {streamId} already exists.");
    internal static ErrorModel InvalidTopicName => new ErrorModel(29, "invalid_topic_name", "Invalid topic name");
    internal static ErrorModel InvalidTopicPartitions => new ErrorModel(30, "invalid_topic_partitions", "Invalid topic partitions");
    internal static ErrorModel LogFileNotFound => new ErrorModel(31, "log_file_not_found", "Log file not found");
    internal static ErrorModel CannotAppendMessage => new ErrorModel(32, "cannot_append_message", "Cannot append message");
    internal static ErrorModel CannotCreatePartition(int streamId, int topicId, int partitionId)
        => new ErrorModel(33, "cannot_create_partition", $"Failed to create partition with ID: {partitionId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreatePartitionDirectory(int streamId, int topicId, int partitionId)
        => new ErrorModel(34, "cannot_create_partition_directory", $"Failed to create directory for partition with ID: {partitionId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreatePartitionSegmentLogFile(int segmentId)
        => new ErrorModel(35, "cannot_create_partition_segment_log_file", $"Failed to create segment log file for segment with ID: {segmentId}.");
    internal static ErrorModel CannotCreatePartitionSegmentIndexFile(int segmentId)
        => new ErrorModel(36, "cannot_create_partition_segment_index_file", $"Failed to create segment index file for segment with ID: {segmentId}.");
    internal static ErrorModel CannotCreatePartitionSegmentTimeIndexFile(int segmentId)
        => new ErrorModel(37, "cannot_create_partition_segment_time_index_file", $"Failed to create segment time index file for segment with ID: {segmentId}.");
    internal static ErrorModel CannotOpenPartitionLogFile => new ErrorModel(38, "cannot_open_partition_log_file", "Failed to open partition log file");
    internal static ErrorModel CannotReadPartitions(int streamId, int topicId)
        => new ErrorModel(39, "cannot_read_partitions", $"Failed to read partitions for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel PartitionNotFound(int partitionId)
        => new ErrorModel(40, "partition_not_found", $"Partition with ID: {partitionId} was not found.");
    internal static ErrorModel InvalidMessagesCount => new ErrorModel(41, "invalid_messages_count", "Invalid messages count");
    internal static ErrorModel InvalidStreamId => new ErrorModel(42, "invalid_stream_id", "Invalid stream ID");
    internal static ErrorModel InvalidTopicId => new ErrorModel(43, "invalid_topic_id", "Invalid topic ID");
    internal static ErrorModel SegmentNotFound => new ErrorModel(44, "segment_not_found", "Segment not found");
    internal static ErrorModel SegmentClosed(int partitionId, int segmentId)
        => new ErrorModel(45, "segment_closed", $"Segment with ID: {segmentId} in partition with ID: {partitionId} is closed.");
    internal static ErrorModel InvalidSegmentSize(int segmentSize)
        => new ErrorModel(46, "invalid_segment_size", $"Invalid segment size: {segmentSize}");
    internal static ErrorModel CannotReadMessage => new ErrorModel(47, "cannot_read_message", "Failed to read message");
    internal static ErrorModel CannotReadMessageTimestamp => new ErrorModel(48, "cannot_read_message_timestamp", "Failed to read message timestamp");
    internal static ErrorModel CannotReadMessageId => new ErrorModel(49, "cannot_read_message_id", "Failed to read message ID");
    internal static ErrorModel CannotReadMessageLength => new ErrorModel(50, "cannot_read_message_length", "Failed to read message length");
    internal static ErrorModel CannotReadMessagePayload => new ErrorModel(51, "cannot_read_message_payload", "Failed to read message payload");
    internal static ErrorModel CannotSaveMessagesToSegment => new ErrorModel(52, "cannot_save_messages_to_segment", "Failed to save messages to segment");
    internal static ErrorModel CannotSaveIndexToSegment => new ErrorModel(53, "cannot_save_index_to_segment", "Failed to save index to segment");
    internal static ErrorModel CannotSaveTimeIndexToSegment => new ErrorModel(54, "cannot_save_time_index_to_segment", "Failed to save time index to segment");
    internal static ErrorModel CannotParseUtf8()
        => new ErrorModel(55, "cannot_parse_utf8", $"Cannot parse UTF8: ");
    internal static ErrorModel CannotParseInt()
        => new ErrorModel(56, "cannot_parse_int", $"Cannot parse integer: ");
    internal static ErrorModel CannotParseSlice()
        => new ErrorModel(57, "cannot_parse_slice", $"Cannot parse slice ");
    internal static ErrorModel TooBigMessagePayload => new ErrorModel(58, "too_big_message_payload", "Too big message payload");
    internal static ErrorModel TooManyMessages => new ErrorModel(59, "too_many_messages", "Too many messages");
    internal static ErrorModel WriteError()
        => new ErrorModel(60, "write_error", $"Write error: ");
    internal static ErrorModel InvalidOffset(ulong offset)
        => new ErrorModel(61, "invalid_offset", $"Invalid offset: {offset}");
    internal static ErrorModel CannotReadConsumerOffsets(int partitionId)
        => new ErrorModel(62, "cannot_read_consumer_offsets", $"Cannot read consumer offsets for partition with ID: {partitionId}");
    internal static ErrorModel CannotDeletePartition(int streamId, int topicId, int partitionId)
        => new ErrorModel(63, "cannot_delete_partition", $"Failed to delete partition with ID: {partitionId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotDeletePartitionDirectory(int streamId, int topicId, int partitionId)
        => new ErrorModel(64, "cannot_delete_partition_directory", $"Failed to delete partition directory with ID: {partitionId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel InvalidMessagePayloadLength => new ErrorModel(65, "invalid_message_payload_length", "Invalid message payload length");
    internal static ErrorModel EmptyMessagePayload => new ErrorModel(67, "empty_message_payload", "Empty message payload");
    internal static ErrorModel CannotReadStreams => new ErrorModel(68, "cannot_read_streams", "Failed to read streams");
    internal static ErrorModel CannotReadTopics(int streamId)
        => new ErrorModel(69, "cannot_read_topics", $"Cannot read topics for stream with ID: {streamId}");
    internal static ErrorModel CannotReadMessageChecksum => new ErrorModel(70, "cannot_read_message_checksum", "Failed to read message checksum");
    internal static ErrorModel InvalidMessageChecksum(int expectedChecksum, int actualChecksum, int messageOffset)
        => new ErrorModel(71, "invalid_message_checksum", $"Invalid message checksum at offset {messageOffset}, expected: {expectedChecksum}, actual: {actualChecksum}");
    internal static ErrorModel ConsumerGroupNotFound(int streamId, int consumerGroupId)
        => new ErrorModel(72, "consumer_group_not_found", $"Consumer group with ID: {consumerGroupId} for stream with ID: {streamId} was not found.");
    internal static ErrorModel ConsumerGroupAlreadyExists(int streamId, int consumerGroupId)
        => new ErrorModel(73, "consumer_group_already_exists", $"Consumer group with ID: {consumerGroupId} for stream with ID: {streamId} already exists.");
    internal static ErrorModel ConsumerGroupMemberNotFound(int streamId, int consumerGroupId, Guid memberId)
        => new ErrorModel(74, "consumer_group_member_not_found", $"Consumer group member with ID: {memberId} for group with ID: {consumerGroupId} and stream with ID: {streamId} was not found.");
    internal static ErrorModel InvalidConsumerGroupId => new ErrorModel(75, "invalid_consumer_group_id", "Invalid consumer group ID");
    internal static ErrorModel FeatureUnavailable => new ErrorModel(76, "feature_unavailable", "Feature unavailable");
    internal static ErrorModel CannotCreatePartitionsDirectory(int streamId, int topicId)
        => new ErrorModel(77, "cannot_create_partitions_directory", $"Failed to create partitions directory for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreateConsumerGroupsDirectory(int streamId, int topicId)
        => new ErrorModel(78, "cannot_create_consumer_groups_directory", $"Failed to create consumer groups directory for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotReadConsumerGroups(int streamId, int topicId)
        => new ErrorModel(79, "cannot_read_consumer_groups", $"Failed to read consumer groups for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotCreateConsumerGroupInfo(int streamId, int topicId, int consumerGroupId)
        => new ErrorModel(80, "cannot_create_consumer_group_info", $"Failed to create consumer group info for group with ID: {consumerGroupId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel CannotDeleteConsumerGroupInfo(int streamId, int topicId, int consumerGroupId)
        => new ErrorModel(81, "cannot_delete_consumer_group_info", $"Failed to delete consumer group info for group with ID: {consumerGroupId} for topic with ID: {topicId} for stream with ID: {streamId}.");
    internal static ErrorModel ClientNotFound(string clientId)
        => new ErrorModel(82, "client_not_found", $"Client with ID: {clientId} was not found.");
    internal static ErrorModel InvalidClientId => new ErrorModel(83, "invalid_client_id", "Invalid client ID");
    internal static ErrorModel Unknown => new ErrorModel(255, "unknown", "Unknown error");
}
