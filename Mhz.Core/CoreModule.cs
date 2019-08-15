using Autofac;
using Module = Autofac.Module;

namespace Mhz.Core
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Config>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Core>()
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues)
                .SingleInstance();
            base.Load(builder);
        }
    }
}