using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace CalendarQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";
        static string TestCallendarId = "j7mn4ovlihgmsud28lct5k2ilc@group.calendar.google.com";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var events = GetEvents(service);
            RemoveEvent(service, events);
            GetEvents(service);
            
            AddEvent(service);
        }

        private static Events GetEvents(CalendarService service)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(TestCallendarId);

            request.TimeMin = DateTime.Now;
            request.TimeMax = DateTime.Now.AddDays(7);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (string.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }

            return events;
        }

        private static void AddEvent(CalendarService service)
        {
            Event newEvent = new Event()
            {
                Summary = "Google I/O 2021",
                Location = "800 Howard St., San Francisco, CA 94103",
                Description = "A chance to hear more about Google's developer products.",
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2021-05-09T09:00:00-07:00"),
                    TimeZone = "America/Los_Angeles",
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2021-05-09T17:00:00-07:00"),
                    TimeZone = "America/Los_Angeles",
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = new EventReminder[] {
            new EventReminder() { Method = "email", Minutes = 60 },
        }
                }
            };

            EventsResource.InsertRequest request = service.Events.Insert(newEvent, TestCallendarId);
            Event createdEvent = request.Execute();
        }

        private static void RemoveEvent(CalendarService service, Events events)
        {
            var request = service.Events.Delete(TestCallendarId, events.Items[0].Id);
            request.Execute();
        }
    }
}