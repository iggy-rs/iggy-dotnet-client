namespace Shared;

public interface ISerializableMessage
{
	public string ToJson();
	public byte[] ToBytes();
}