#! "dnx451"
#r "nuget:Microsoft.Azure.WebJobs,*"


using System;


public static void Run(string myEventHubMessage)
{
    // log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
    Console.WriteLine("Test");
}

Run("test");
