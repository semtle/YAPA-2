﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using YAPA.Contracts;
using YAPA.Shared;
using YAPA.Shared.Contracts;
using YAPA.WPF.Plugins;

namespace YAPA.WPF
{
    public class DependencyContainer
    {
        public IContainer Container { get; private set; }
        public IPluginManager PluginManager { get; private set; }
        public IThemeManager ThemeManager { get; private set; }
        public Dashboard Dashboard { get; private set; }
        public Window MainWindow { get; private set; }

        public DependencyContainer()
        {
            Container = ConfigureContainer();

            var di = new DependencyInjector(Container);
            di.RegisterInstance(di, typeof(IDependencyInjector));

            //Themes
            ThemeManager = new ThemeManager(di, GetThemeMetas(), (ThemeManagerSettings)Container.Resolve(typeof(ThemeManagerSettings)));
            var themeUpdater = new ContainerBuilder();
            themeUpdater.RegisterInstance(ThemeManager).As<IThemeManager>().SingleInstance();
            themeUpdater.RegisterType(ThemeManager.ActiveTheme.Theme).As<IApplication>().SingleInstance();
            themeUpdater.Update(Container);

            //Plugins
            PluginManager = new PluginManager(di, GetPluginMetas(), (PluginManagerSettings)Container.Resolve(typeof(PluginManagerSettings)));
            var updater = new ContainerBuilder();
            updater.RegisterInstance(PluginManager).As<IPluginManager>().SingleInstance();
            updater.Update(Container);

            PluginManager.InitPlugins();

            Dashboard = Container.Resolve<Dashboard>();

            MainWindow = (Window)Container.Resolve<IApplication>();
        }

        private static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<JsonYapaSettings>().As<ISettings>().AutoActivate().SingleInstance();
            builder.RegisterType<NewftonsJson>().As<IJson>().SingleInstance();

            builder.RegisterType(typeof(PomodoroEngine)).As<IPomodoroEngine>().SingleInstance();
            builder.RegisterType(typeof(PomodoroEngineSettings)).As<PomodoroEngineSettings>();

            builder.RegisterType(typeof(Timer)).As<ITimer>();
            builder.RegisterType(typeof(SoundPlayer)).As<IMusicPlayer>();

            builder.RegisterType(typeof(MainViewModel)).As<IMainViewModel>();

            builder.RegisterType(typeof(ThemeManagerSettings));
            //builder.RegisterType(typeof(ThemeManagerSettingWindow));

            builder.RegisterType(typeof(PluginManagerSettings));
            builder.RegisterType(typeof(PluginManagerSettingWindow));

            builder.RegisterType(typeof(GithubDashboard));
            builder.RegisterType(typeof(Dashboard)).SingleInstance();

            builder.RegisterType(typeof(WpfEnviroment)).As<IEnviroment>().SingleInstance();

            builder.RegisterType(typeof(ItemRepository)).As<IPomodoroRepository>().SingleInstance();

            builder.RegisterType(typeof(ShowSettingsCommand)).As<IShowSettingsCommand>();


            builder.RegisterType(typeof(SettingManager)).As<ISettingManager>().SingleInstance();

            var container = builder.Build();

            var updater = new ContainerBuilder();
            updater.RegisterInstance(container).As<IContainer>();
            updater.Update(container);

            return container;
        }

        private static IEnumerable<IPluginMeta> GetPluginMetas()
        {
            foreach (IPluginMeta meta in from plugin in Assembly.GetExecutingAssembly().GetTypes()
                                         where plugin.GetInterfaces().Contains(typeof(IPluginMeta))
                                         select (IPluginMeta)Activator.CreateInstance(plugin))
            {
                yield return meta;
            }

            foreach (IPluginMeta meta in from plugin in Assembly.LoadFrom("YAPA.WPF.Shared.dll").GetTypes()
                                         where plugin.GetInterfaces().Contains(typeof(IPluginMeta))
                                         select (IPluginMeta)Activator.CreateInstance(plugin))
            {
                yield return meta;
            }
        }

        private static IEnumerable<IThemeMeta> GetThemeMetas()
        {
            foreach (IThemeMeta meta in from plugin in Assembly.GetExecutingAssembly().GetTypes()
                                        where plugin.GetInterfaces().Contains(typeof(IThemeMeta))
                                        select (IThemeMeta)Activator.CreateInstance(plugin))
            {
                yield return meta;
            }

            foreach (IThemeMeta meta in from plugin in Assembly.LoadFrom("YAPA.WPF.Shared.dll").GetTypes()
                                        where plugin.GetInterfaces().Contains(typeof(IThemeMeta))
                                        select (IThemeMeta)Activator.CreateInstance(plugin))
            {
                yield return meta;
            }

            foreach (IThemeMeta meta in from plugin in Assembly.LoadFrom("YAPA.WPF.Themes.dll").GetTypes()
                                        where plugin.GetInterfaces().Contains(typeof(IThemeMeta))
                                        select (IThemeMeta)Activator.CreateInstance(plugin))
            {
                yield return meta;
            }
        }
    }
}