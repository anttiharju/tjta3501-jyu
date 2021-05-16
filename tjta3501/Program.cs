namespace tjta3501
{
    public class Program
    {
        public static void Main()
        {
            CommandEngine engine = new CommandEngine();
            new Hoyry(engine);
            engine.Run();
        }
    }
}
