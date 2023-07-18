using Iggy_SDK.Utils;

namespace Iggy_SDK_Tests.Utils.Errors;

public static class ErrorModelFactory
{
	public static ErrorModel CreateErrorModelBadRequest()
	{
		return new ErrorModel
		{
			Id = 69,
			Code = "bad_request",
			Reason = "Bad Request"
		};
	}

	public static ErrorModel CreateErrorModelNotFound()
	{
		return new ErrorModel
		{
			Id = 69,
			Code = "not_found",
			Reason = "Not Found"
		};
	}
}