namespace DNSProfileChecker.Common
{
	public interface IValidator<T>
	{
		bool Validate(T input);

		T MissedValues { get; }
	}
}