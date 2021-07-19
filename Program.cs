using System;

class Program
{
    static void Main(string[] args)
    {
        foreach (var cookie in new EdgeCookieReader().GetEdgeCookies())
        {
            Console.WriteLine(cookie);
        }
    }
}