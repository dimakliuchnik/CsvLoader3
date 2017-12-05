using System.Web.Mvc;
using CsvLoader3.Controllers;
using CsvLoader3.Models;
using Microsoft.Practices.Unity;
using Unity.Mvc3;

namespace CsvLoader3
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<IUnitOfWork, MongoUnitOfWork>();
            container.RegisterType<IController, FilesController>("Files");
            container.RegisterType<IController, LoginController>("LoginPassword");
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();            

            return container;
        }
    }
}