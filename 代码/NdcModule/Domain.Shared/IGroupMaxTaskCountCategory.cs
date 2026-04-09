using Volo.Abp.DependencyInjection;

namespace AciModule.Domain.Shared
{
    public interface IGroupMaxTaskCountCategory
    {
        int GetMaxTaskCount(string GroupName);
    }

    public class DefaultGroupMaxTaskCountCategory : IGroupMaxTaskCountCategory,ITransientDependency
    {
        public int GetMaxTaskCount(string GroupName)
        {
            return 1000;
        }
    }
}
