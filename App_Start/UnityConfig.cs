using BillManagementSoftware.Repository.DbConfig;
using BillManagementSoftware.Repository.Interface;
using BillManagementSoftware.Repository.Service;
using System.Web.Http;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace CherukarasThejas
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IMysqlData, MysqlData>();
            container.RegisterType<IProductMethod, ProductMethod>();
            container.RegisterType<IBillMethod, BillMethod>();

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            // Set the dependency resolver for MVC (if using both MVC and Web API)
            DependencyResolver.SetResolver(new Unity.Mvc5.UnityDependencyResolver(container));
        }
    }
}