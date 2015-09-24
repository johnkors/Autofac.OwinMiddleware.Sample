using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Owin;
using Owin;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

[assembly: OwinStartup(typeof(OwinMiddleWare.Startup))]

namespace OwinMiddleWare
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SomeComponent>();
            builder.RegisterType<CustomMwInheriting>();
            builder.RegisterType<CustomMwWithoutInheritance>(); // Is not registered as middleware by Autofac because of missing inheritence, and hence not run
            var container = builder.Build();

            app.UseAutofacMiddleware(container); // runs all registered types inheriting from 'OwinMiddleware'
            //app.Use<CustomMwWithoutInheritance>(); Crashes
        }
    }

    public class CustomMwInheriting : OwinMiddleware
    {
        private readonly SomeComponent _component;

        public CustomMwInheriting(OwinMiddleware next, SomeComponent component) : base(next)
        {
            _component = component;
        }

        public override async Task Invoke(IOwinContext ctx)
        {
            await ctx.Response.WriteAsync(_component.SayHi("inheritor"));
        }
    }

    public class CustomMwWithoutInheritance 
    {
        private readonly AppFunc _next;
        private readonly SomeComponent _component;

        public CustomMwWithoutInheritance(AppFunc next, SomeComponent component)
        {
            _next = next;
            _component = component;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            await context.Response.WriteAsync(_component.SayHi("no inheritor"));
            await _next(env);
        }
    }

    public class SomeComponent
    {
        public string SayHi(string middleware)
        {
            return "Hi, " + middleware;
        }
    }
}
