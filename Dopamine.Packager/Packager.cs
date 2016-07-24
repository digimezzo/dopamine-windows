namespace Dopamine.Packager
{
    class Packager
    {
        static void Main(string[] args)
        {
            var worker = new PackagerWorker();
            worker.Execute();
        }
    }
}
