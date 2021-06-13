using static SpotifyAPI.Web.LoginRequest;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

namespace SpotifySongAvailabilityChecker_Console
{
    class Program
    {
        static EmbedIOAuthServer server;
        static SpotifyClient client;
        static FullTrack track;

        static void Main(string[] args)
        {
            string input;
            Console.WriteLine($"{Environment.NewLine}Spotify Song Availability Checker - Console (Version 1.0), Created by CasualHonest{Environment.NewLine}");

            do
            {
                Console.Write(
                    $"What would you like to do today? Your options are:{Environment.NewLine}" +
                    $"0: Find availability for a song{Environment.NewLine}" +
                    $"1: Find availability for an album{Environment.NewLine}" +
                    $"2: Generate an access token (Required for finding item availability){Environment.NewLine}" +
                    $"3: View your search history (Not implemented yet){Environment.NewLine}" +
                    $"4: Quit application{Environment.NewLine}" +
                    $"{Environment.NewLine}Your choice: ");

                input = Console.ReadLine();
                Console.WriteLine();

            } while (!uint.TryParse(input, out uint _));

            uint val = uint.Parse(input);

            switch (val)
            {
                case 0:
                    if (client == null)
                    {
                        Console.WriteLine($"Please generate an access token before continuing{Environment.NewLine}");
                        Console.WriteLine("Press any key to continue...");
                    }
                    else
                    {
                        Console.Write("Enter in a song URL here: ");
                        string songUrl = Console.ReadLine().Trim();
                        string id = "";

                        try
                        {
                            int beginIndex = songUrl.LastIndexOf("/") + 1;
                            int endIndex = songUrl.IndexOf("?si=") - 1;
                            id = songUrl.Substring(beginIndex, endIndex - beginIndex + 1);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine($"The program was not able to parse {songUrl} into a song ID{Environment.NewLine}");
                            Console.WriteLine("Press any key to continue...");
                            ReadKey();
                        }

                        try
                        {
                            track = client.Tracks.Get(id).Result;
                        }
                        catch (AggregateException ae)
                        {
                            Exception ex = ae.GetBaseException();
                            if (ex is APIException)
                            {
                                Console.WriteLine($"The program was not able to retrieve {id} as it does not exist{Environment.NewLine}");
                            }
                            else if (ex is APIUnauthorizedException)
                            {
                                Console.WriteLine($"The program was not able to retrieve {id} as the current access token has expired{Environment.NewLine}");
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine(
                                    $"There was an error with converting the URL to a song track{Environment.NewLine}" +
                                    $"Error: {ex}{Environment.NewLine}");
                            }
                            Console.WriteLine("Press any key to continue...");
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine($"The program was not able to parse {id} as it is not in the right format{Environment.NewLine}");
                            Console.WriteLine("Press any key to continue...");
                            ReadKey();
                        }

                        Console.WriteLine($"{Environment.NewLine}INFORMATION:");
                        Console.WriteLine($"Track Name: {track.Name}{Environment.NewLine}");

                        string authors = "";
                        foreach (SimpleArtist artist in track.Artists)
                        {
                            authors += $"{artist.Name}, ";
                        }
                        authors = authors.Substring(0, authors.Length - 2);
                        Console.WriteLine($"Authors: {authors}");

                        string availableMarkets = "";
                        foreach (string market in track.AvailableMarkets)
                        {
                            availableMarkets += $"{market}{Environment.NewLine}";
                        }
                        Console.WriteLine($"{Environment.NewLine}Available Markets:{Environment.NewLine}{availableMarkets}");
                    }
                    break;
                case 1:
                    if (client == null)
                    {
                        Console.WriteLine($"Please generate an access token before continuing{Environment.NewLine}");
                        Console.WriteLine("Press any key to continue...");
                    }
                    else
                    {
                        Console.Write("Enter in a song URL here: ");
                        string songUrl = Console.ReadLine().Trim();
                        string id = "";

                        try
                        {
                            int beginIndex = songUrl.LastIndexOf("/") + 1;
                            int endIndex = songUrl.IndexOf("?si=") - 1;
                            id = songUrl.Substring(beginIndex, endIndex - beginIndex + 1);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine($"The program was not able to parse {songUrl} into a song ID{Environment.NewLine}");
                            Console.WriteLine("Press any key to continue...");
                            ReadKey();
                        }

                        try
                        {
                            track = client.Tracks.Get(id).Result;
                        }
                        catch (AggregateException ae)
                        {
                            Exception ex = ae.GetBaseException();
                            if (ex is APIException)
                            {
                                Console.WriteLine($"The program was not able to retrieve {id} as it does not exist{Environment.NewLine}");
                            }
                            else if (ex is APIUnauthorizedException)
                            {
                                Console.WriteLine($"The program was not able to retrieve {id} as the current access token has expired{Environment.NewLine}");
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine(
                                    $"There was an error with converting the URL to a song track{Environment.NewLine}" +
                                    $"Error: {ex}{Environment.NewLine}");
                            }
                            Console.WriteLine("Press any key to continue...");
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine($"The program was not able to parse {id} as it is not in the right format{Environment.NewLine}");
                            Console.WriteLine("Press any key to continue...");
                            ReadKey();
                        }

                        Console.WriteLine($"{Environment.NewLine}INFORMATION:");
                        Console.WriteLine($"Track Name: {track.Name}{Environment.NewLine}");

                        string authors = "";
                        foreach (SimpleArtist artist in track.Artists)
                        {
                            authors += $"{artist.Name}, ";
                        }
                        authors = authors.Substring(0, authors.Length - 2);
                        Console.WriteLine($"Authors: {authors}");

                        string availableMarkets = "";
                        foreach (string market in track.AvailableMarkets)
                        {
                            availableMarkets += $"{market}{Environment.NewLine}";
                        }
                        Console.WriteLine($"{Environment.NewLine}Available Markets:{Environment.NewLine}{availableMarkets}");
                    }
                    break;
                case 2:
                    StartServer();
                    Console.Clear();
                    Console.WriteLine("Waiting for access to be given...");
                    break;
                case 3:
                    break;
                case 4:
                    Environment.Exit(0);
                    break;
            }
            ReadKey();
        }

        private static async void StartServer()
        {
            server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await server.Start();

            server.ImplictGrantReceived += OnReceivedPermission;
            server.ErrorReceived += OnReceivedError;

            LoginRequest request = new LoginRequest(server.BaseUri, "4f8281c491f244d0a4b2058dbb4587a6", ResponseType.Token);
            Uri requestToUri = request.ToUri();
            BrowserUtil.Open(requestToUri);
        }

        private static async Task OnReceivedPermission(object sender, ImplictGrantResponse response)
        {
            server.ImplictGrantReceived -= OnReceivedPermission;
            await server.Stop();
            client = new SpotifyClient(response.AccessToken);
            Console.Clear();
            Console.WriteLine(
                $"{Environment.NewLine}Access token generated! Now you can check availability of items on Spotify{Environment.NewLine}" +
                $"{Environment.NewLine}The token will expire in {response.ExpiresIn} seconds{Environment.NewLine}" +
                $"{Environment.NewLine}Press any key to continue...");

        }

        private static async Task OnReceivedError(object sender, string error, string state)
        {
            server.ErrorReceived -= OnReceivedError;
            await server.Stop();
            Console.WriteLine(
                $"There was an error with authorizing the application{Environment.NewLine}" +
                $"Error: {error}{Environment.NewLine}" +
                $"State: {state}{Environment.NewLine}");
        }

        private static void ReadKey()
        {
            _ = Console.ReadKey().KeyChar;
            Console.Clear();
            Main(Array.Empty<string>());
        }
    }
}
