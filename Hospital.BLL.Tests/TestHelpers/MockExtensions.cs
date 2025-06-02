using Moq.Language.Flow;

namespace Hospital.BLL.Tests.TestHelpers
{
    public static class MockExtensions
    {
        public static IReturnsResult<T> ReturnsAsync<T, TResult>(
            this ISetup<T, Task<TResult>> setup,
            TResult result) where T : class
        {
            return setup.Returns(Task.FromResult(result));
        }

        public static IReturnsResult<T> ReturnsAsync<T, TResult>(
            this ISetup<T, Task<IEnumerable<TResult>>> setup,
            IEnumerable<TResult> results) where T : class
        {
            return setup.Returns(Task.FromResult(results));
        }
    }
}