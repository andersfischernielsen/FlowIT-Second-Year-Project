using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Common.Tools;
using Event.Communicators;
using Event.Controllers;
using Event.Interfaces;
using Event.Logic;
using Event.Storage;
using Microsoft.Practices.Unity;

namespace Event
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<IEventContext, EventContext>();

            // Communicator:
            container.RegisterType<HttpClient>(new InjectionConstructor());
            container.RegisterType<IHttpClient, HttpClientWrapper>(
                new InjectionConstructor(container.Resolve<HttpClient>()));
            container.RegisterType<HttpClientToolbox>(new InjectionConstructor(container.Resolve<IHttpClient>()));
            container.RegisterType<IEventFromEvent, EventCommunicator>(
                new InjectionConstructor(container.Resolve<HttpClientToolbox>()));

            // Storage:
            container.RegisterType<IEventStorage, EventStorage>(
                new InjectionConstructor(container.Resolve<IEventContext>()));
            container.RegisterType<IEventHistoryStorage, EventStorage>(
                new InjectionConstructor(container.Resolve<IEventContext>()));
            container.RegisterType<IEventStorageForReset, EventStorageForReset>(
                new InjectionConstructor(container.Resolve<IEventContext>()));

            // Logics:
            container.RegisterType<IAuthLogic, AuthLogic>(new InjectionConstructor(container.Resolve<IEventStorage>()));
            container.RegisterType<IEventHistoryLogic, EventHistoryLogic>(
                new InjectionConstructor(container.Resolve<IEventHistoryStorage>()));
            container.RegisterType<ILockingLogic, LockingLogic>(
                new InjectionConstructor(container.Resolve<IEventStorage>(), container.Resolve<IEventFromEvent>()));
            container.RegisterType<ILifecycleLogic, LifecycleLogic>(
                new InjectionConstructor(container.Resolve<IEventStorage>(), container.Resolve<IEventStorageForReset>(),
                    container.Resolve<ILockingLogic>()));
            container.RegisterType<IStateLogic, StateLogic>(new InjectionConstructor(
                container.Resolve<IEventStorage>(), container.Resolve<ILockingLogic>(), container.Resolve<IAuthLogic>(),
                container.Resolve<IEventFromEvent>()));

            // Controllers:
            container.RegisterType<HistoryController>(new InjectionConstructor(container.Resolve<IEventHistoryLogic>()));
            container.RegisterType<LifecycleController>(new InjectionConstructor(container.Resolve<ILifecycleLogic>(),
                container.Resolve<IEventHistoryLogic>()));
            container.RegisterType<StateController>(new InjectionConstructor(container.Resolve<IStateLogic>(),
                container.Resolve<IEventHistoryLogic>()));
            container.RegisterType<LockController>(new InjectionConstructor(container.Resolve<ILockingLogic>(),
                container.Resolve<IEventHistoryLogic>()));

            config.DependencyResolver = new DependencyResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "EventApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
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
