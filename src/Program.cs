using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(new EdgeCookieReader().GetEdgeCookie(".cloud.huawei.com", "CSRFToken"));
    }
}