// See https://aka.ms/new-console-template for more information
using WebPush;

Console.WriteLine("Hello, World!");

var vapidKeys = VapidHelper.GenerateVapidKeys();

// Prints 2 URL Safe Base64 Encoded Strings
Console.WriteLine("Public {0}", vapidKeys.PublicKey);
Console.WriteLine("Private {0}", vapidKeys.PrivateKey);

Console.ReadLine();
