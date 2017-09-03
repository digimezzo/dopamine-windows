using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Core.Base;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace Dopamine.Common.Services.JumpList
{
    public class JumpListService : IJumpListService
    {
        #region Variables
        private System.Windows.Shell.JumpList jumpList;
        #endregion

        #region Construction
        public JumpListService()
        {
            this.jumpList = System.Windows.Shell.JumpList.GetJumpList(Application.Current);
        }
        #endregion

        #region IJumpListService
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
                        Arguments = "/donate " + ContactInformation.PayPalLink,
                        Description = "",
                        IconResourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Defaults.IconsLibrary),
                        ApplicationPath = Assembly.GetEntryAssembly().Location,
                        IconResourceIndex = 0
                    });
                }

            });

            if (this.jumpList != null) this.jumpList.Apply();
        }
        #endregion
    }
}
