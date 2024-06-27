using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Services.JumpList;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace Dopamine.Services.JumpList
{
    public class JumpListService : IJumpListService
    {
        private System.Windows.Shell.JumpList jumpList;
      
        public JumpListService()
        {
            this.jumpList = System.Windows.Shell.JumpList.GetJumpList(Application.Current);
        }
       
        public async void PopulateJumpListAsync()
        {
            await Task.Run(() =>
            {
                if (this.jumpList != null)
                {
                    this.jumpList.JumpItems.Clear();
                    this.jumpList.ShowFrequentCategory = false;
                    this.jumpList.ShowRecentCategory = false;

                    this.jumpList.JumpItems.Add(new JumpTask
                    {
                        Title = ResourceUtils.GetString("Language_Donate"),
                        Arguments = "/donate " + ContactInformation.DonateLink,
                        Description = "",
                        IconResourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Defaults.IconsLibrary + ".dll"),
                        ApplicationPath = Assembly.GetEntryAssembly().Location,
                        IconResourceIndex = 0
                    });
                }

            });

            if (this.jumpList != null) this.jumpList.Apply();
        }
    }
}
