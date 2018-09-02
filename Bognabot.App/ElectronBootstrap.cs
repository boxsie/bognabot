using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace Bognabot.App
{
    public static class ElectronBootstrap
    {
        public static async Task InitAsync()
        {
            await CreateWindow();

            CreateMenu();
        }

        private static async Task CreateWindow()
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 1152,
                Height = 864,
                Show = false
            });

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle("Bognabot");
        }

        private static void CreateMenu()
        {
            var menu = new MenuItem[]
            {
                new MenuItem
                {
                    Label = "Edit",
                    Submenu = new MenuItem[]
                    {
                        new MenuItem {Label = "Undo", Accelerator = "CmdOrCtrl+Z", Role = MenuRole.undo},
                        new MenuItem {Label = "Redo", Accelerator = "Shift+CmdOrCtrl+Z", Role = MenuRole.redo},
                        new MenuItem {Type = MenuType.separator},
                        new MenuItem {Label = "Cut", Accelerator = "CmdOrCtrl+X", Role = MenuRole.cut},
                        new MenuItem {Label = "Copy", Accelerator = "CmdOrCtrl+C", Role = MenuRole.copy},
                        new MenuItem {Label = "Paste", Accelerator = "CmdOrCtrl+V", Role = MenuRole.paste},
                        new MenuItem {Label = "Select All", Accelerator = "CmdOrCtrl+A", Role = MenuRole.selectall}
                    }
                },
                new MenuItem
                {
                    Label = "View",
                    Submenu = new MenuItem[]
                    {
                        new MenuItem
                        {
                            Label = "Reload",
                            Accelerator = "CmdOrCtrl+R",
                            Click = () =>
                            {
                                // on reload, start fresh and close any old
                                // open secondary windows
                                Electron.WindowManager.BrowserWindows.ToList().ForEach(browserWindow =>
                                {
                                    if (browserWindow.Id != 1)
                                    {
                                        browserWindow.Close();
                                    }
                                    else
                                    {
                                        browserWindow.Reload();
                                    }
                                });
                            }
                        },
                        new MenuItem
                        {
                            Label = "Toggle Full Screen",
                            Accelerator = "CmdOrCtrl+F",
                            Click = async () =>
                            {
                                bool isFullScreen =
                                    await Electron.WindowManager.BrowserWindows.First().IsFullScreenAsync();
                                Electron.WindowManager.BrowserWindows.First().SetFullScreen(!isFullScreen);
                            }
                        },
                        new MenuItem
                        {
                            Label = "Open Developer Tools",
                            Accelerator = "CmdOrCtrl+I",
                            Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                        },
                        new MenuItem
                        {
                            Label = "Refresh",
                            Accelerator = "CmdOrCtrl+F5",
                            Click = () => Electron.WindowManager.BrowserWindows.First().Reload()
                        },
                    }
                },
                new MenuItem
                {
                    Label = "Window",
                    Role = MenuRole.window,
                    Submenu = new MenuItem[]
                    {
                        new MenuItem {Label = "Minimize", Accelerator = "CmdOrCtrl+M", Role = MenuRole.minimize},
                        new MenuItem {Label = "Close", Accelerator = "CmdOrCtrl+W", Role = MenuRole.close}
                    }
                },
                new MenuItem
                {
                    Label = "Help",
                    Role = MenuRole.help,
                    Submenu = new MenuItem[]
                    {
                        new MenuItem
                        {
                            Label = "Learn More",
                            Click = async () => await Electron.Shell.OpenExternalAsync("https://github.com/ElectronNET")
                        }
                    }
                }
            };

            Electron.Menu.SetApplicationMenu(menu);
        }
    }
}
