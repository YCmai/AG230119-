using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.Reflection;

namespace TaskBaseModule.Application
{
    public class TaskHandleTypeMap : Volo.Abp.DependencyInjection.ISingletonDependency
    {
        private readonly ITypeFinder _typeFinder;

        Dictionary<string, Type> _TypeMap = new(); //第一个任务对象，第二个是对象的handleType

        public TaskHandleTypeMap(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public void InialMap()
        {
            var taskhandleType = typeof(IParentTaskHandle);
			foreach (var type in _typeFinder.Types) //查找所有模块的类型
			{
				if (taskhandleType.IsAssignableFrom(type))  //t可以转换成taskhandleType
				{
					foreach (var typeInterface in type.GetInterfaces())  //检查type的所有接口
					{
						if (typeInterface.IsGenericType && typeInterface.GetGenericTypeDefinition() == typeof(IParentTaskHandle<>)) //接口是否指定的泛型类型
						{
							var GenericArgType = typeInterface.GetGenericArguments().FirstOrDefault();  //获取泛型的参数类型
							if (GenericArgType != null)
							{
								_TypeMap.TryAdd(GenericArgType.FullName, type);
							}
							break;
						}
					}
				}

			}
		}

		public Type GetTaskHandleType(Type taskType)
		{
			Type type;
			_TypeMap.TryGetValue(taskType.FullName, out type);
			return type;
		}
    }
}
