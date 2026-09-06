using YesBankCoreUI;

namespace YESBankUAT
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
           // Application.Run(new Ekyc());
            //Application.Run(new DMT());
           // Application.Run(new AEPS());
            //  Application.Run(new Ekyc());
           Application.Run(new SORYesBankPayout());
         //   Application.Run(new DMT());
        }
    }
}