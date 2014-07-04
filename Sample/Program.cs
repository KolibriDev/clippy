using Kolibri;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Clippy.PushStringToClipboard(args[0]);
                System.Console.WriteLine("pushed \"{0}\" to the clipboard.",args[0]);
            }
            else
            {
                System.Console.WriteLine("usage: sample.exe \"<message>\"");
                System.Console.WriteLine("       pushes the message <message> onto the clipboard.");
            }
        }
    }
}
