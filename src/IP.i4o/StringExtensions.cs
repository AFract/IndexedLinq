namespace DotNetProjects.IndexedLinq
{
	internal static class StringExtensions
	{
		internal static string FormatWith(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
	}
}