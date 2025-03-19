

static class Program
{
    static void Main()
    {
        GameController.I = new GameController();
        GameController.I.MainThread();   
    }
}