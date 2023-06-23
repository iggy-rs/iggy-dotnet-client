using System.Diagnostics;

namespace Iggy_SDK.Extensions;

internal static class Extensions
{
   internal static string ToSnakeCase(this string input)
   {
       Debug.Assert(!string.IsNullOrEmpty(input));
       if (CountUppercaseLetters(input) == 0)
	       return input.ToLower();
       
       var len = input.Length + CountUppercaseLetters(input) - 1;
       return string.Create(len, input, (span, value) =>
       {
	      value.AsSpan().CopyTo(span);
	      span[0] = char.ToLower(span[0]);
	      
	      for (int i = 0; i < len; ++i)
	      {
		      if (char.IsUpper(span[i]))
		      {
			      span[i] = char.ToLower(span[i]);
			      span[i..].ShiftSliceRight();
			      span[i] = '_';
		      }
	      }
       });
   }
   private static int CountUppercaseLetters(string input)
   {
	   return input.Count(char.IsUpper);
   }
   private static void ShiftSliceRight(this Span<char> slice)
   {
	   for (int i = slice.Length - 2; i >= 0; i--)
	   {
		   slice[i + 1] = slice[i];
	   }
   }
}