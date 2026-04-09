using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TaskBaseModule.Domain.Shared
{
	public interface IParentTaskHandle<ParentT>:IParentTaskHandle,ITransientDependency where ParentT : TaskBaseAggregateRoot
	{

	}
}
