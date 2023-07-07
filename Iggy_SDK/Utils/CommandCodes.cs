namespace ConsoleApp;
//TODO - change this to internal later
public static class CommandCodes
{
	public const byte KILL_CODE = 0;
	public const byte PING_CODE = 1;
	public const byte GET_CLIENTS_CODE = 2;
	public const byte SEND_MESSAGES_CODE = 10;
	public const byte POLL_MESSAGES_CODE = 11;
	public const byte STORE_OFFSET_CODE = 12;
	public const byte GET_OFFSET_CODE = 13;
	public const byte GET_STREAM_CODE = 20;
	public const byte GET_STREAMS_CODE = 21;
	public const byte CREATE_STREAM_CODE = 22;
	public const byte DELETE_STREAM_CODE = 23;
	public const byte GET_TOPIC_CODE = 30;
	public const byte GET_TOPICS_CODE = 31;
	public const byte CREATE_TOPIC_CODE = 32;
	public const byte DELETE_TOPIC_CODE = 33;
	public const byte GET_GROUP_CODE = 40;
	public const byte GET_GROUPS_CODE = 41;
	public const byte CREATE_GROUP_CODE = 42;
	public const byte DELETE_GROUP_CODE = 43;
	public const byte JOIN_GROUP_CODE = 44;
	public const byte LEAVE_GROUP_CODE = 45;
}
