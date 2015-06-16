using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;
using Server.Controllers;
using Server.Interfaces;
using Server.Logic;
using Server.Storage;

namespace Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();

            container.RegisterType<IServerContext, StorageContext>();
            container.RegisterType<IServerStorage, ServerStorage>(
                new InjectionConstructor(container.Resolve<IServerContext>()));
            container.RegisterType<IServerHistoryStorage, ServerStorage>(
                new InjectionConstructor(container.Resolve<IServerContext>()));
            container.RegisterType<IServerLogic, ServerLogic>(
                new InjectionConstructor(container.Resolve<IServerStorage>()));
            container.RegisterType<IWorkflowHistoryLogic, WorkflowHistoryLogic>(
                new InjectionConstructor(container.Resolve<IServerHistoryStorage>()));

            // Register controllers, to make it possible for the framework to know the types of arguments.
            container.RegisterType<HistoryController>(
                new InjectionConstructor(container.Resolve<IWorkflowHistoryLogic>()));
            container.RegisterType<UsersController>(new InjectionConstructor(container.Resolve<IServerLogic>(),
                container.Resolve<IWorkflowHistoryLogic>()));
            container.RegisterType<WorkflowsController>(new InjectionConstructor(container.Resolve<IServerLogic>(),
                container.Resolve<IWorkflowHistoryLogic>()));

            config.DependencyResolver = new DependencyResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private class DependencyResolver : IDependencyResolver
        {
            private readonly IUnityContainer _container;

            public DependencyResolver(IUnityContainer container)
            {
                _container = container;
            }

            public void Dispose()
            {
                _container.Dispose();
            }

            public object GetService(Type serviceType)
            {
                try
                {
                    return _container.Resolve(serviceType);
                }
                catch (ResolutionFailedException)
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                try
                {
                    return _container.ResolveAll(serviceType);
                }
                catch (ResolutionFailedException)
                {
                    return new List<object>();
                }
            }

            public IDependencyScope BeginScope()
            {
                return new DependencyResolver(_container.CreateChildContainer());
            }
        }
    }
}
