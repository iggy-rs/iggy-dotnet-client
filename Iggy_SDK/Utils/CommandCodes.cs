namespace Iggy_SDK.Utils;

internal static class CommandCodes
{
	internal const byte KILL_CODE = 0;
	internal const byte PING_CODE = 1;
	internal const byte GET_CLIENTS_CODE = 2;
	internal const byte SEND_MESSAGES_CODE = 10;
	internal const byte POLL_MESSAGES_CODE = 11;
	internal const byte STORE_OFFSET_CODE = 12;
	internal const byte GET_OFFSET_CODE = 13;
	internal const byte GET_STREAM_CODE = 20;
	internal const byte GET_STREAMS_CODE = 21;
	internal const byte CREATE_STREAM_CODE = 22;
	internal const byte DELETE_STREAM_CODE = 23;
	internal const byte GET_TOPIC_CODE = 30;
	internal const byte GET_TOPICS_CODE = 31;
	internal const byte CREATE_TOPIC_CODE = 32;
	internal const byte DELETE_TOPIC_CODE = 33;
	internal const byte GET_GROUP_CODE = 40;
	internal const byte GET_GROUPS_CODE = 41;
	internal const byte CREATE_GROUP_CODE = 42;
	internal const byte DELETE_GROUP_CODE = 43;
	internal const byte JOIN_GROUP_CODE = 44;
	internal const byte LEAVE_GROUP_CODE = 45;
}
